using System;

namespace CassMach.Domain.Entities
{
    public class TokenTransaction : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public string TransactionType { get; set; }
        public long RawTokens { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal? Multiplier { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; }
        public Guid? ReferenceId { get; set; }

        public User User { get; set; }
    }
}
