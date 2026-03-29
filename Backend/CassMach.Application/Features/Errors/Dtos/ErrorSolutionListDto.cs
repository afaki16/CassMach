using System;

namespace CassMach.Application.Features.Errors.Dtos
{
    public class ErrorSolutionListDto
    {
        public int Id { get; set; }
        public Guid ConversationId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string ErrorCode { get; set; }
        public string UserQuestion { get; set; }
        public string AiResponse { get; set; }
        public int AttemptNumber { get; set; }
        public bool? IsAccepted { get; set; }
        public bool FromCache { get; set; }
        public decimal CreditsCharged { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
