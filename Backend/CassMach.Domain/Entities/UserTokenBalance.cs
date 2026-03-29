using System;

namespace CassMach.Domain.Entities
{
    public class UserTokenBalance : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public long TotalRawTokensUsed { get; set; }
        public decimal TotalCreditsUsed { get; set; }

        public User User { get; set; }
    }
}
