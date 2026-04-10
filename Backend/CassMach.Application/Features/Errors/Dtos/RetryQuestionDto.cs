namespace CassMach.Application.Features.Errors.Dtos
{
    public class RetryQuestionDto
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ErrorCode { get; set; }
        public string? Symptom { get; set; }
        public bool SkipEnrichment { get; set; }
    }
}
