using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using MediatR;

namespace CassMach.Application.Features.Admin.Commands.TopUpTokens
{
    public class TopUpTokensCommand : IRequest<Result<TokenBalanceDto>>
    {
        public int UserId { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; }
        public int AdminUserId { get; set; }
    }
}
