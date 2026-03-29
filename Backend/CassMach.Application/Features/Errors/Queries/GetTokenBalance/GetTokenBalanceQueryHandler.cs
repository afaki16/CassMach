using AutoMapper;
using CassMach.Application.Common.Interfaces;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Errors.Queries.GetTokenBalance
{
    public class GetTokenBalanceQueryHandler : IRequestHandler<GetTokenBalanceQuery, Result<TokenBalanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public GetTokenBalanceQueryHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<Result<TokenBalanceDto>> Handle(GetTokenBalanceQuery request, CancellationToken cancellationToken)
        {
            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(request.UserId);

            if (balance == null)
            {
                await _tokenService.EnsureBalanceExists(request.UserId);
                balance = await _unitOfWork.UserTokenBalances.GetByUserId(request.UserId);
            }

            var dto = _mapper.Map<TokenBalanceDto>(balance);
            return Result<TokenBalanceDto>.Success(dto);
        }
    }
}
