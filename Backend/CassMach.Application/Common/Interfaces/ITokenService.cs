using System;
using System.Threading.Tasks;

namespace CassMach.Application.Common.Interfaces
{
    public interface ITokenService
    {
        Task<bool> HasSufficientBalance(int userId);
        Task<decimal> GetBalance(int userId);
        Task EnsureBalanceExists(int userId);
        Task ChargeForAiResponse(int userId, int inputTokens, int outputTokens, Guid conversationId, string description);
        Task ChargeForCachedResponse(int userId, Guid conversationId, string description);
        Task TopUp(int userId, decimal creditAmount, string description, int adminUserId);
        Task GiftTokens(int userId, decimal creditAmount, string description, int adminUserId);
    }
}
