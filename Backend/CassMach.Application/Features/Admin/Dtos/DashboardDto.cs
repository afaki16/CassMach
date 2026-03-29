namespace CassMach.Application.Features.Admin.Dtos
{
    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAcceptedSolutions { get; set; }
        public int TotalCachedResponses { get; set; }
        public long TotalTokensUsed { get; set; }
        public decimal TotalCreditsUsed { get; set; }
    }
}
