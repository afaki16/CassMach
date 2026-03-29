using System;
using System.Collections.Generic;

namespace CassMach.Application.Features.Admin.Dtos
{
    public class UserUsageReportDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal CurrentBalance { get; set; }
        public long TotalRawTokensUsed { get; set; }
        public decimal TotalCreditsUsed { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAcceptedSolutions { get; set; }
        public List<TokenTransactionDto> RecentTransactions { get; set; }
    }

    public class TokenTransactionDto
    {
        public string TransactionType { get; set; }
        public long RawTokens { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
