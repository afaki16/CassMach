using Anthropic;
using Anthropic.Models.Messages;
using CassMach.Application.Common.Interfaces;
using CassMach.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Infrastructure.Services
{
    public class ClaudeService : IClaudeService
    {
        private readonly AnthropicClient _client;
        private readonly ClaudeApiSettings _settings;
        private readonly SemaphoreSlim _semaphore;
        private readonly ILogger<ClaudeService> _logger;

        private const string PARSE_SYSTEM_PROMPT = @"Sen bir endüstriyel makine hata kodu analiz asistanısın.
Kullanıcının metninden şu bilgileri çıkar ve SADECE JSON dön, başka hiçbir şey yazma:
{
  ""brand"": ""makine markası veya null"",
  ""model"": ""makine modeli veya null"",
  ""error_code"": ""hata kodu veya null"",
  ""symptom"": ""belirti/semptom açıklaması veya null"",
  ""is_valid"": true veya false
}

ÖNEMLİ KURALLAR:
- is_valid alanı SADECE konunun endüstriyel/makine ile ilgili olup olmadığını belirler.
- Kullanıcı ""makine"", ""tezgah"", ""CNC"", ""motor"", ""ışık yanıyor"", ""alarm veriyor"", ""hata kodu"" gibi endüstriyel terimler kullandıysa is_valid MUTLAKA true olmalıdır.
- Marka belirtilmemiş olması is_valid değerini ETKİLEMEZ. Marka yoksa brand=null yap ama is_valid=true kalsın.
- Hata kodu belirtilmemiş olması is_valid değerini ETKİLEMEZ. Kod yoksa error_code=null yap ama is_valid=true kalsın.

Geçerli konular: makine, PLC, CNC, robot, üretim hattı, hidrolik, pnömatik, servo motor, elektrik panosu, kompresör, konveyör, tezgah, torna, freze ve benzeri endüstriyel sistemler.

Geçersiz konular (is_valid=false): yazılım, web, telefon, bilgisayar, günlük yaşam, sağlık veya endüstri dışı her şey.

Eğer kullanıcı makine markası belirtmediyse brand alanını null olarak dön. Marka bilgisi olmadan çözüm üretilemez.";

        private const string SOLUTION_SYSTEM_PROMPT = @"Sen bir endüstriyel makine hata kodu uzmanısın.
Kullanıcının dilinde cevap ver.
Sadece makine/endüstriyel hatalarla ilgili çözüm üret.
Adım adım çözüm öner.
Güvenlik uyarılarını mutlaka belirt.
Eğer hata kodunu kesin tanımıyorsan, olası nedenleri ve genel kontrol adımlarını öner.
Cevabını teknik ama anlaşılır tut.";

        private const string RETRY_SYSTEM_PROMPT = @"Sen bir endüstriyel makine hata kodu uzmanısın.
Kullanıcının dilinde cevap ver.
Kullanıcı önceki çözümü/çözümleri beğenmedi.
Farklı bir yaklaşımla alternatif çözüm öner.
Aynı adımları tekrarlama.
Mümkünse farklı bir kök neden analizi yap.
Güvenlik uyarılarını mutlaka belirt.";

        public ClaudeService(IOptions<ClaudeApiSettings> settings, ILogger<ClaudeService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _client = new AnthropicClient
            {
                ApiKey = _settings.ApiKey,
                Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds),
                MaxRetries = _settings.MaxRetryAttempts
            };
            _semaphore = new SemaphoreSlim(_settings.MaxConcurrentRequests);
        }

        public async Task<ParseResult> ParseUserQuestion(string question, CancellationToken cancellationToken = default)
        {
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(_settings.TimeoutSeconds), cancellationToken))
                throw new TimeoutException("Claude API concurrency limit reached");

            try
            {
                var parameters = new MessageCreateParams
                {
                    Model = _settings.Model,
                    MaxTokens = 1024,
                    System = PARSE_SYSTEM_PROMPT,
                    Messages = [new() { Role = Role.User, Content = question }]
                };

                var message = await _client.Messages.Create(parameters, cancellationToken: cancellationToken);
                var fullResponse = message.ToString();

                var textContent = ExtractTextContent(fullResponse);

                var jsonText = textContent.Trim();
                jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, @"^```(?:json)?\s*", "");
                jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, @"\s*```$", "");
                jsonText = jsonText.Trim();

                var jsonStart = jsonText.IndexOf('{');
                var jsonEnd = jsonText.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                    jsonText = jsonText.Substring(jsonStart, jsonEnd - jsonStart + 1);

                try
                {
                    var parsed = JsonSerializer.Deserialize<JsonElement>(jsonText);
                    return new ParseResult
                    {
                        Brand = parsed.TryGetProperty("brand", out var brand) && brand.ValueKind != JsonValueKind.Null
                            ? brand.GetString() : null,
                        Model = parsed.TryGetProperty("model", out var model) && model.ValueKind != JsonValueKind.Null
                            ? model.GetString() : null,
                        ErrorCode = parsed.TryGetProperty("error_code", out var errorCode) && errorCode.ValueKind != JsonValueKind.Null
                            ? errorCode.GetString() : null,
                        Symptom = parsed.TryGetProperty("symptom", out var symptom) && symptom.ValueKind != JsonValueKind.Null
                            ? symptom.GetString() : null,
                        IsValid = parsed.TryGetProperty("is_valid", out var isValid) && isValid.GetBoolean()
                    };
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Failed to parse Claude response as JSON: {Response}", textContent);
                    return new ParseResult { IsValid = false };
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async IAsyncEnumerable<StreamResult> StreamSolution(
            string question, string brand, string model, string errorCode, string symptom,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var userMessage = BuildSolutionPrompt(question, brand, model, errorCode, symptom);

            await foreach (var result in StreamFromClaude(SOLUTION_SYSTEM_PROMPT, userMessage, cancellationToken))
            {
                yield return result;
            }
        }

        public async IAsyncEnumerable<StreamResult> StreamRetrySolution(
            string question, string brand, string model, string errorCode, string symptom,
            List<string> previousResponses,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var userMessage = BuildRetryPrompt(question, brand, model, errorCode, symptom, previousResponses);

            await foreach (var result in StreamFromClaude(RETRY_SYSTEM_PROMPT, userMessage, cancellationToken))
            {
                yield return result;
            }
        }

        private async IAsyncEnumerable<StreamResult> StreamFromClaude(
            string systemPrompt, string userMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(_settings.TimeoutSeconds), cancellationToken))
                throw new TimeoutException("Claude API concurrency limit reached");

            try
            {
                var parameters = new MessageCreateParams
                {
                    Model = _settings.Model,
                    MaxTokens = 4096,
                    System = systemPrompt,
                    Messages = [new() { Role = Role.User, Content = userMessage }]
                };

                var fullText = new StringBuilder();
                int inputTokens = 0;
                int outputTokens = 0;

                await foreach (var evt in _client.Messages.CreateStreaming(parameters, cancellationToken: cancellationToken))
                {
                    if (evt.TryPickStart(out var startEvent))
                    {
                        inputTokens = (int)startEvent.Message.Usage.InputTokens;
                    }
                    else if (evt.TryPickDelta(out var messageDeltaEvent))
                    {
                        outputTokens = (int)messageDeltaEvent.Usage.OutputTokens;
                    }

                    var text = ExtractStreamText(evt.ToString());
                    if (!string.IsNullOrEmpty(text))
                    {
                        fullText.Append(text);
                        yield return new StreamResult
                        {
                            Text = text,
                            IsDone = false,
                            InputTokens = 0,
                            OutputTokens = 0
                        };
                    }
                }

                yield return new StreamResult
                {
                    Text = string.Empty,
                    IsDone = true,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static string BuildSolutionPrompt(string question, string brand, string model, string errorCode, string symptom)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Kullanıcı sorusu: {question}");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(brand)) sb.AppendLine($"Marka: {brand}");
            if (!string.IsNullOrEmpty(model)) sb.AppendLine($"Model: {model}");
            if (!string.IsNullOrEmpty(errorCode)) sb.AppendLine($"Hata Kodu: {errorCode}");
            if (!string.IsNullOrEmpty(symptom)) sb.AppendLine($"Semptom: {symptom}");
            return sb.ToString();
        }

        private static string BuildRetryPrompt(string question, string brand, string model, string errorCode, string symptom, List<string> previousResponses)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Kullanıcı sorusu: {question}");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(brand)) sb.AppendLine($"Marka: {brand}");
            if (!string.IsNullOrEmpty(model)) sb.AppendLine($"Model: {model}");
            if (!string.IsNullOrEmpty(errorCode)) sb.AppendLine($"Hata Kodu: {errorCode}");
            if (!string.IsNullOrEmpty(symptom)) sb.AppendLine($"Semptom: {symptom}");

            if (previousResponses?.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("--- ÖNCEKİ CEVAPLAR (bunlardan farklı bir çözüm öner) ---");
                for (int i = 0; i < previousResponses.Count; i++)
                {
                    sb.AppendLine($"Cevap {i + 1}:");
                    sb.AppendLine(previousResponses[i]);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static string ExtractStreamText(string eventJson)
        {
            try
            {
                var doc = JsonSerializer.Deserialize<JsonElement>(eventJson);

                if (doc.TryGetProperty("type", out var eventType))
                {
                    var type = eventType.GetString();

                    if (type == "content_block_delta" &&
                        doc.TryGetProperty("delta", out var delta) &&
                        delta.TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? "";
                    }
                }
            }
            catch
            {
                // not JSON or unexpected format
            }
            return null;
        }

        private static string ExtractTextContent(string fullResponse)
        {
            try
            {
                var doc = JsonSerializer.Deserialize<JsonElement>(fullResponse);
                if (doc.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                {
                    foreach (var block in content.EnumerateArray())
                    {
                        if (block.TryGetProperty("type", out var blockType) &&
                            blockType.GetString() == "text" &&
                            block.TryGetProperty("text", out var text))
                        {
                            return text.GetString() ?? "";
                        }
                    }
                }
            }
            catch
            {
                // fallback: not a JSON envelope, treat as raw text
            }
            return fullResponse;
        }
    }
}
