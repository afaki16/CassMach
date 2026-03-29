using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using MediatR;

namespace CassMach.Application.Features.Errors.Queries.GetTokenBalance
{
    public class GetTokenBalanceQuery : IRequest<Result<TokenBalanceDto>>
    {
        public int UserId { get; set; }
    }
}
