namespace CassMach.Application.Features.Admin.Dtos
{
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal TokenBalance { get; set; }
        public decimal TotalCreditsUsed { get; set; }
        public long TotalRawTokensUsed { get; set; }
    }
}
