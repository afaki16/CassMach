using CassMach.Application.Common.Interfaces;
using CassMach.Application.Features.Errors.Commands.AcceptSolution;
using CassMach.Application.Features.Errors.Dtos;
using CassMach.Application.Features.Errors.Queries.GetErrorHistory;
using CassMach.Application.Features.Errors.Queries.GetTokenBalance;
using CassMach.Domain.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

                var parseResult = await _claudeService.ParseUserQuestion(dto.Question);

                if (!parseResult.IsValid)
                {
                    await WriteSSEEvent(new { type = "error", message = "Bu soru makine hatası ile ilgili değil. Sadece endüstriyel makine hatalarına cevap verebilirim." });
                    return;
                }

                await WriteSSEEvent(new { type = "parse", brand = parseResult.Brand, errorCode = parseResult.ErrorCode, model = parseResult.Model });

                var cachedSolution = await _unitOfWork.ErrorSolutions.GetAcceptedSolution(parseResult.Brand, parseResult.ErrorCode);
                var conversationId = Guid.NewGuid();

                if (cachedSolution != null)
                {
                    var words = cachedSolution.AiResponse.Split(' ');
                    foreach (var word in words)
                    {
                        await WriteSSEEvent(new { type = "chunk", text = word + " " });
                        await Task.Delay(30);
                    }

                    await _tokenService.ChargeForCachedResponse(userId, conversationId, $"Cache: {parseResult.Brand} {parseResult.ErrorCode}");

                    var dbFixedCreditSetting = await _unitOfWork.SystemSettings.GetByKey("db_fixed_credit");
                    var creditsCharged = decimal.Parse(dbFixedCreditSetting.Value);

                    var cachedRecord = new Domain.Entities.ErrorSolution
                    {
                        UserId = userId,
                        ConversationId = conversationId,
                        UserQuestion = dto.Question,
                        Brand = parseResult.Brand,
                        Model = parseResult.Model,
                        ErrorCode = parseResult.ErrorCode,
                        Symptom = parseResult.Symptom,
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
                        dto.Question, parseResult.Brand, parseResult.Model, parseResult.ErrorCode, parseResult.Symptom))
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

                    await _tokenService.ChargeForAiResponse(userId, inputTokens, outputTokens, conversationId, $"AI: {parseResult.Brand} {parseResult.ErrorCode}");

                    var multiplierSetting = await _unitOfWork.SystemSettings.GetByKey("token_multiplier");
                    var multiplier = decimal.Parse(multiplierSetting.Value);
                    var creditsCharged = (inputTokens + outputTokens) * multiplier;

                    var aiRecord = new Domain.Entities.ErrorSolution
                    {
                        UserId = userId,
                        ConversationId = conversationId,
                        UserQuestion = dto.Question,
                        Brand = parseResult.Brand,
                        Model = parseResult.Model,
                        ErrorCode = parseResult.ErrorCode,
                        Symptom = parseResult.Symptom,
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
                await WriteSSEEvent(new { type = "error", message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{conversationId:guid}/retry")]
        [Authorize(Policy = "errors.create")]
        public async Task RetryQuestion(Guid conversationId)
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

                var previousResponses = existingAttempts
                    .Where(a => !a.FromCache)
                    .Select(a => a.AiResponse)
                    .ToList();

                var fullResponse = new System.Text.StringBuilder();
                int inputTokens = 0, outputTokens = 0;

                await foreach (var streamResult in _claudeService.StreamRetrySolution(
                    firstAttempt.UserQuestion, firstAttempt.Brand, firstAttempt.Model,
                    firstAttempt.ErrorCode, firstAttempt.Symptom, previousResponses))
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

                await _tokenService.ChargeForAiResponse(userId, inputTokens, outputTokens, conversationId, $"Retry: {firstAttempt.Brand} {firstAttempt.ErrorCode}");

                var multiplierSetting = await _unitOfWork.SystemSettings.GetByKey("token_multiplier");
                var multiplier = decimal.Parse(multiplierSetting.Value);
                var creditsCharged = (inputTokens + outputTokens) * multiplier;

                var newAttempt = new Domain.Entities.ErrorSolution
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    UserQuestion = firstAttempt.UserQuestion,
                    Brand = firstAttempt.Brand,
                    Model = firstAttempt.Model,
                    ErrorCode = firstAttempt.ErrorCode,
                    Symptom = firstAttempt.Symptom,
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
                await WriteSSEEvent(new { type = "error", message = "Bir hata oluştu: " + ex.Message });
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
    }
}
