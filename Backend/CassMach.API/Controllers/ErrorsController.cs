using CassMach.Application.Common.Interfaces;
using CassMach.Application.Features.Errors.Commands.AcceptSolution;
using CassMach.Application.Features.Errors.Dtos;
using CassMach.Application.Features.Errors.Queries.GetConversation;
using CassMach.Application.Features.Errors.Queries.GetErrorHistory;
using CassMach.Application.Features.Errors.Queries.GetTokenBalance;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

namespace CassMach.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IClaudeService _claudeService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        public ErrorsController(
            IMediator mediator,
            IClaudeService claudeService,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _claudeService = claudeService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("ask")]
        [Authorize(Policy = "errors.create")]
        public async Task AskQuestion([FromBody] AskQuestionDto dto)
        {
            var userId = GetCurrentUserId();

            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            try
            {
                await _tokenService.EnsureBalanceExists(userId);
                if (!await _tokenService.HasSufficientBalance(userId))
                {
                    await WriteSSEEvent(new { type = "error", message = "Token bakiyeniz yetersiz" });
                    return;
                }

                string brand, model, errorCode, symptom;
                int? machineId = null;
                bool hasMachine = dto.MachineId.HasValue;

                if (hasMachine)
                {
                    // Makine seçildi → UserMachines üzerinden brand/model çek, parse etme
                    var userMachine = await _unitOfWork.UserMachines.GetByIdAndUserId(dto.MachineId.Value, userId);
                    if (userMachine == null)
                    {
                        await WriteSSEEvent(new { type = "error", message = "Seçilen makine bulunamadı" });
                        return;
                    }
                    machineId = userMachine.MachineId;
                    brand = userMachine.Machine.Brand;
                    model = userMachine.Machine.Model;
                    // Hata kodu ve belirti sorudan çıkarılır (kısa parse, sadece errorCode+symptom)
                    var quickParse = await _claudeService.ParseUserQuestion(dto.Question);
                    errorCode = quickParse.ErrorCode;
                    symptom = quickParse.Symptom;

                    await WriteSSEEvent(new { type = "parse", brand, errorCode, model });
                }
                else
                {
                    // Makine seçilmedi → tam parse
                    var parseResult = await _claudeService.ParseUserQuestion(dto.Question);

                    if (!parseResult.IsValid)
                    {
                        await WriteSSEEvent(new { type = "error", message = "Bu soru makine hatası ile ilgili değil. Sadece endüstriyel makine hatalarına cevap verebilirim." });
                        return;
                    }

                    var hasBrand = !string.IsNullOrWhiteSpace(parseResult.Brand);
                    var hasErrorCode = !string.IsNullOrWhiteSpace(parseResult.ErrorCode);
                    var hasSymptom = !string.IsNullOrWhiteSpace(parseResult.Symptom);
                    if (!hasBrand && !hasErrorCode && !hasSymptom)
                    {
                        await WriteSSEEvent(new { type = "error", message = "Lütfen makine markası, hata kodu veya belirti belirtin. Örnek: 'Lenze 5115 hatası' veya 'Siemens CNC'de kırmızı ışık yanıyor'" });
                        return;
                    }

                    brand = parseResult.Brand;
                    model = parseResult.Model;
                    errorCode = parseResult.ErrorCode;
                    symptom = parseResult.Symptom;

                    await WriteSSEEvent(new { type = "parse", brand, errorCode, model });
                }

                // Cache kontrolü
                ErrorSolution? cachedSolution = null;
                if (!string.IsNullOrWhiteSpace(brand))
                    cachedSolution = await _unitOfWork.ErrorSolutions.GetAcceptedSolution(brand, errorCode);

                var conversationId = Guid.NewGuid();

                // Multiplier: makine seçilmediyse penalty uygulanır
                var multiplierSetting = await _unitOfWork.SystemSettings.GetByKey("token_multiplier");
                var baseMultiplier = decimal.Parse(multiplierSetting.Value);
                decimal penaltyFactor = 1m;
                if (!hasMachine)
                {
                    var penaltySetting = await _unitOfWork.SystemSettings.GetByKey("no_machine_penalty");
                    if (penaltySetting != null)
                        penaltyFactor = decimal.Parse(penaltySetting.Value);
                }
                var effectiveMultiplier = baseMultiplier * penaltyFactor;

                if (cachedSolution != null)
                {
                    var words = cachedSolution.AiResponse.Split(' ');
                    foreach (var word in words)
                    {
                        await WriteSSEEvent(new { type = "chunk", text = word + " " });
                        await Task.Delay(30);
                    }

                    await _tokenService.ChargeForCachedResponse(userId, conversationId, $"Cache: {brand} {errorCode}");

                    var dbFixedCreditSetting = await _unitOfWork.SystemSettings.GetByKey("db_fixed_credit");
                    var creditsCharged = decimal.Parse(dbFixedCreditSetting.Value) * penaltyFactor;

                    var cachedRecord = new Domain.Entities.ErrorSolution
                    {
                        UserId = userId,
                        MachineId = machineId,
                        ConversationId = conversationId,
                        UserQuestion = dto.Question,
                        Brand = brand ?? string.Empty,
                        Model = model,
                        ErrorCode = errorCode,
                        Symptom = symptom,
                        AiResponse = cachedSolution.AiResponse,
                        AttemptNumber = 1,
                        FromCache = true,
                        InputTokens = 0,
                        OutputTokens = 0,
                        CreditsCharged = creditsCharged
                    };
                    await _unitOfWork.ErrorSolutions.AddAsync(cachedRecord);
                    await _unitOfWork.SaveChangesAsync();

                    var maxRetrySetting = await _unitOfWork.SystemSettings.GetByKey("max_retry_count");
                    var maxRetry = int.Parse(maxRetrySetting.Value);
                    var balance = await _tokenService.GetBalance(userId);

                    await WriteSSEEvent(new { type = "done", conversationId, attempt = 1, creditsCharged = cachedRecord.CreditsCharged, remainingBalance = balance, remainingRetries = maxRetry - 1 });
                }
                else
                {
                    var fullResponse = new System.Text.StringBuilder();
                    int inputTokens = 0, outputTokens = 0;

                    await foreach (var streamResult in _claudeService.StreamSolution(
                        dto.Question, brand, model, errorCode, symptom))
                    {
                        if (!streamResult.IsDone)
                        {
                            fullResponse.Append(streamResult.Text);
                            await WriteSSEEvent(new { type = "chunk", text = streamResult.Text });
                        }
                        else
                        {
                            inputTokens = streamResult.InputTokens;
                            outputTokens = streamResult.OutputTokens;
                        }
                    }

                    await _tokenService.ChargeForAiResponse(userId, inputTokens, outputTokens, conversationId, $"AI: {brand} {errorCode}");

                    var creditsCharged = (inputTokens + outputTokens) * effectiveMultiplier;

                    var aiRecord = new ErrorSolution
                    {
                        UserId = userId,
                        MachineId = machineId,
                        ConversationId = conversationId,
                        UserQuestion = dto.Question,
                        Brand = brand ?? string.Empty,
                        Model = model,
                        ErrorCode = errorCode,
                        Symptom = symptom,
                        AiResponse = fullResponse.ToString(),
                        AttemptNumber = 1,
                        FromCache = false,
                        InputTokens = inputTokens,
                        OutputTokens = outputTokens,
                        CreditsCharged = creditsCharged
                    };
                    await _unitOfWork.ErrorSolutions.AddAsync(aiRecord);
                    await _unitOfWork.SaveChangesAsync();

                    var maxRetrySetting = await _unitOfWork.SystemSettings.GetByKey("max_retry_count");
                    var maxRetry = int.Parse(maxRetrySetting.Value);
                    var balance = await _tokenService.GetBalance(userId);

                    await WriteSSEEvent(new { type = "done", conversationId, attempt = 1, creditsCharged, remainingBalance = balance, remainingRetries = maxRetry - 1 });
                }
            }
            catch (Exception ex)
            {
                await WriteSSEEvent(new { type = "error", message = ResolveErrorMessage(ex) });
            }
        }

        [HttpPost("{conversationId:guid}/retry")]
        [Authorize(Policy = "errors.create")]
        public async Task RetryQuestion(Guid conversationId, [FromBody] RetryQuestionDto? dto)
        {
            var userId = GetCurrentUserId();

            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            try
            {
                var existingAttempts = await _unitOfWork.ErrorSolutions.GetByConversationId(conversationId, userId);
                if (existingAttempts == null || existingAttempts.Count == 0)
                {
                    await WriteSSEEvent(new { type = "error", message = "Conversation bulunamadı" });
                    return;
                }

                var maxRetrySetting = await _unitOfWork.SystemSettings.GetByKey("max_retry_count");
                var maxRetry = int.Parse(maxRetrySetting.Value);
                var currentAttempt = existingAttempts.Count;

                if (currentAttempt >= maxRetry)
                {
                    await WriteSSEEvent(new { type = "error", message = $"Maksimum deneme sayısına ({maxRetry}) ulaştınız" });
                    return;
                }

                if (!await _tokenService.HasSufficientBalance(userId))
                {
                    await WriteSSEEvent(new { type = "error", message = "Token bakiyeniz yetersiz" });
                    return;
                }

                var firstAttempt = existingAttempts[0];

                // Mevcut kayıttaki bilgileri al
                var brand = firstAttempt.Brand;
                var model = firstAttempt.Model;
                var errorCode = firstAttempt.ErrorCode;
                var symptom = firstAttempt.Symptom;

                // Eğer enrichment body'si geldiyse merge et
                if (dto != null && !dto.SkipEnrichment)
                {
                    // Kullanıcı devam mesajı yazdıysa önce onu parse et, sonra merge et
                    if (!string.IsNullOrWhiteSpace(dto.ContinuationQuestion))
                    {
                        var parsed = await _claudeService.ParseUserQuestion(dto.ContinuationQuestion);
                        if (!string.IsNullOrWhiteSpace(parsed.Brand)) brand = parsed.Brand;
                        if (!string.IsNullOrWhiteSpace(parsed.Model)) model = parsed.Model;
                        if (!string.IsNullOrWhiteSpace(parsed.ErrorCode)) errorCode = parsed.ErrorCode;
                        if (!string.IsNullOrWhiteSpace(parsed.Symptom)) symptom = parsed.Symptom;
                    }

                    // Manuel doldurulan alanlar ContinuationQuestion'ı override eder
                    if (!string.IsNullOrWhiteSpace(dto.Brand)) brand = dto.Brand;
                    if (!string.IsNullOrWhiteSpace(dto.Model)) model = dto.Model;
                    if (!string.IsNullOrWhiteSpace(dto.ErrorCode)) errorCode = dto.ErrorCode;
                    if (!string.IsNullOrWhiteSpace(dto.Symptom)) symptom = dto.Symptom;
                }

                // Body gelmemiş (ilk retry tıklaması) VE eksik alan var mı kontrol et
                var isFirstRetryClick = dto == null;
                if (isFirstRetryClick)
                {
                    var missingFields = new List<string>();
                    if (string.IsNullOrWhiteSpace(brand)) missingFields.Add("brand");
                    if (string.IsNullOrWhiteSpace(model)) missingFields.Add("model");
                    if (string.IsNullOrWhiteSpace(errorCode)) missingFields.Add("errorCode");
                    if (string.IsNullOrWhiteSpace(symptom)) missingFields.Add("symptom");

                    if (missingFields.Count > 0)
                    {
                        await WriteSSEEvent(new { type = "needs_info", missingFields });
                        return;
                    }
                }

                var previousResponses = existingAttempts
                    .Where(a => !a.FromCache)
                    .Select(a => a.AiResponse)
                    .ToList();

                var fullResponse = new System.Text.StringBuilder();
                int inputTokens = 0, outputTokens = 0;

                await foreach (var streamResult in _claudeService.StreamRetrySolution(
                    firstAttempt.UserQuestion, brand, model, errorCode, symptom, previousResponses))
                {
                    if (!streamResult.IsDone)
                    {
                        fullResponse.Append(streamResult.Text);
                        await WriteSSEEvent(new { type = "chunk", text = streamResult.Text });
                    }
                    else
                    {
                        inputTokens = streamResult.InputTokens;
                        outputTokens = streamResult.OutputTokens;
                    }
                }

                foreach (var attempt in existingAttempts.Where(a => a.IsAccepted == null))
                {
                    attempt.IsAccepted = false;
                    _unitOfWork.ErrorSolutions.Update(attempt);
                }

                await _tokenService.ChargeForAiResponse(userId, inputTokens, outputTokens, conversationId, $"Retry: {brand} {errorCode}");

                var multiplierSetting = await _unitOfWork.SystemSettings.GetByKey("token_multiplier");
                var multiplier = decimal.Parse(multiplierSetting.Value);
                var creditsCharged = (inputTokens + outputTokens) * multiplier;

                var newAttempt = new ErrorSolution
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    UserQuestion = firstAttempt.UserQuestion,
                    Brand = brand ?? string.Empty,
                    Model = model,
                    ErrorCode = errorCode,
                    Symptom = symptom,
                    AiResponse = fullResponse.ToString(),
                    AttemptNumber = currentAttempt + 1,
                    FromCache = false,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens,
                    CreditsCharged = creditsCharged
                };
                await _unitOfWork.ErrorSolutions.AddAsync(newAttempt);
                await _unitOfWork.SaveChangesAsync();

                var balance = await _tokenService.GetBalance(userId);

                await WriteSSEEvent(new { type = "done", conversationId, attempt = currentAttempt + 1, creditsCharged, remainingBalance = balance, remainingRetries = maxRetry - (currentAttempt + 1) });
            }
            catch (Exception ex)
            {
                await WriteSSEEvent(new { type = "error", message = ResolveErrorMessage(ex) });
            }
        }

        [HttpPatch("{conversationId:guid}/accept")]
        [Authorize(Policy = "errors.update")]
        public async Task<IActionResult> AcceptSolution(Guid conversationId, [FromBody] AcceptSolutionDto dto)
        {
            var command = new AcceptSolutionCommand
            {
                ConversationId = conversationId,
                AttemptNumber = dto.AttemptNumber,
                UserId = GetCurrentUserId()
            };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("history")]
        [Authorize(Policy = "errors.read")]
        public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTerm = null)
        {
            var query = new GetErrorHistoryQuery
            {
                UserId = GetCurrentUserId(),
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query);
            return HandlePagedResult(result);
        }

        [HttpGet("{conversationId:guid}")]
        [Authorize(Policy = "errors.read")]
        public async Task<IActionResult> GetConversation(Guid conversationId)
        {
            var query = new GetConversationQuery
            {
                ConversationId = conversationId,
                UserId = GetCurrentUserId()
            };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("balance")]
        [Authorize(Policy = "errors.read")]
        public async Task<IActionResult> GetBalance()
        {
            var query = new GetTokenBalanceQuery { UserId = GetCurrentUserId() };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        private async Task WriteSSEEvent(object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await Response.WriteAsync($"data: {json}\n\n");
            await Response.Body.FlushAsync();
        }

        private static string ResolveErrorMessage(Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            if (message.Contains("overloaded_error") || message.Contains("overloaded"))
                return "Claude API şu an yoğun, lütfen birkaç saniye bekleyip tekrar deneyin.";
            if (message.Contains("rate_limit") || message.Contains("429"))
                return "İstek limiti aşıldı, lütfen kısa süre sonra tekrar deneyin.";
            if (message.Contains("timeout") || message.Contains("Timeout"))
                return "Yanıt süresi aşıldı, lütfen tekrar deneyin.";
            return $"Bir hata oluştu: {message}";
        }
    }
}
