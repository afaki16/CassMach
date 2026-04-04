using CassMach.Application.Common.Interfaces;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace CassMach.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> HasSufficientBalance(int userId)
        {
            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            return balance != null && balance.Balance > 0;
        }

        public async Task<decimal> GetBalance(int userId)
        {
            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            return balance?.Balance ?? 0;
        }

        public async Task EnsureBalanceExists(int userId)
        {
            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            if (balance != null)
                return;

            var giftSetting = await _unitOfWork.SystemSettings.GetByKey("default_gift_tokens");
            var giftAmount = giftSetting != null ? decimal.Parse(giftSetting.Value) : 5000m;

            var newBalance = new UserTokenBalance
            {
                UserId = userId,
                Balance = giftAmount,
                TotalRawTokensUsed = 0,
                TotalCreditsUsed = 0
            };

            await _unitOfWork.UserTokenBalances.AddAsync(newBalance);
            await _unitOfWork.SaveChangesAsync();

            var transaction = new TokenTransaction
            {
                UserId = userId,
                TransactionType = "gift",
                RawTokens = 0,
                CreditAmount = giftAmount,
                Multiplier = null,
                BalanceAfter = giftAmount,
                Description = "Initial gift tokens",
                ReferenceId = null
            };

            await _unitOfWork.TokenTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChargeForAiResponse(int userId, int inputTokens, int outputTokens, Guid conversationId, string description)
        {
            var multiplierSetting = await _unitOfWork.SystemSettings.GetByKey("token_multiplier");
            var multiplier = multiplierSetting != null ? decimal.Parse(multiplierSetting.Value) : 1m;

            var totalRawTokens = inputTokens + outputTokens;
            var creditCharge = totalRawTokens * multiplier;

            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            if (balance == null)
            {
                await EnsureBalanceExists(userId);
                balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            }

            balance.Balance -= creditCharge;
            balance.TotalRawTokensUsed += totalRawTokens;
            balance.TotalCreditsUsed += creditCharge;
            _unitOfWork.UserTokenBalances.Update(balance);

            var transaction = new TokenTransaction
            {
                UserId = userId,
                TransactionType = "usage",
                RawTokens = totalRawTokens,
                CreditAmount = -creditCharge,
                Multiplier = multiplier,
                BalanceAfter = balance.Balance,
                Description = description,
                ReferenceId = conversationId
            };

            await _unitOfWork.TokenTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChargeForCachedResponse(int userId, Guid conversationId, string description)
        {
            var fixedCreditSetting = await _unitOfWork.SystemSettings.GetByKey("db_fixed_credit");
            var fixedCredit = fixedCreditSetting != null ? decimal.Parse(fixedCreditSetting.Value) : 25m;

            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            if (balance == null)
            {
                await EnsureBalanceExists(userId);
                balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            }

            balance.Balance -= fixedCredit;
            balance.TotalCreditsUsed += fixedCredit;
            _unitOfWork.UserTokenBalances.Update(balance);

            var transaction = new TokenTransaction
            {
                UserId = userId,
                TransactionType = "usage",
                RawTokens = 0,
                CreditAmount = -fixedCredit,
                Multiplier = null,
                BalanceAfter = balance.Balance,
                Description = description,
                ReferenceId = conversationId
            };

            await _unitOfWork.TokenTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task TopUp(int userId, decimal creditAmount, string description, int adminUserId)
        {
            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            if (balance == null)
            {
                await EnsureBalanceExists(userId);
                balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            }

            balance.Balance += creditAmount;
            _unitOfWork.UserTokenBalances.Update(balance);

            var transaction = new TokenTransaction
            {
                UserId = userId,
                TransactionType = "topup",
                RawTokens = 0,
                CreditAmount = creditAmount,
                Multiplier = null,
                BalanceAfter = balance.Balance,
                Description = description,
                ReferenceId = null
            };

            await _unitOfWork.TokenTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task GiftTokens(int userId, decimal creditAmount, string description, int adminUserId)
        {
            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            if (balance == null)
            {
                await EnsureBalanceExists(userId);
                balance = await _unitOfWork.UserTokenBalances.GetByUserId(userId);
            }

            balance.Balance += creditAmount;
            _unitOfWork.UserTokenBalances.Update(balance);

            var transaction = new TokenTransaction
            {
                UserId = userId,
                TransactionType = "gift",
                RawTokens = 0,
                CreditAmount = creditAmount,
                Multiplier = null,
                BalanceAfter = balance.Balance,
                Description = description,
                ReferenceId = null
            };

            await _unitOfWork.TokenTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
