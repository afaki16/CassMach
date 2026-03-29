using System;

namespace CassMach.Domain.Entities
{
    public class ErrorSolution : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public Guid ConversationId { get; set; }
        public string UserQuestion { get; set; }
        public string Brand { get; set; }
        public string? Model { get; set; }
        public string? ErrorCode { get; set; }
        public string? Symptom { get; set; }
        public string AiResponse { get; set; }
        public int AttemptNumber { get; set; }
        public bool? IsAccepted { get; set; }
        public bool FromCache { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public decimal CreditsCharged { get; set; }

        public User User { get; set; }
    }
}
