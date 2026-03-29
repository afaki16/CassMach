namespace CassMach.Application.Features.Errors.Dtos
{
    public class TokenBalanceDto
    {
        public decimal Balance { get; set; }
        public long TotalRawTokensUsed { get; set; }
        public decimal TotalCreditsUsed { get; set; }
    }
}
