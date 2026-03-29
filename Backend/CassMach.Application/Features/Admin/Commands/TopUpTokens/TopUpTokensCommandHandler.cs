using AutoMapper;
using CassMach.Application.Common.Interfaces;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Admin.Commands.TopUpTokens
{
    public class TopUpTokensCommandHandler : IRequestHandler<TopUpTokensCommand, Result<TokenBalanceDto>>
    {
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TopUpTokensCommandHandler(ITokenService tokenService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TokenBalanceDto>> Handle(TopUpTokensCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
                return Result<TokenBalanceDto>.Failure(Error.Failure(ErrorCode.NotFound, "User not found"));

            await _tokenService.TopUp(request.UserId, request.CreditAmount, request.Description, request.AdminUserId);

            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(request.UserId);
            var dto = _mapper.Map<TokenBalanceDto>(balance);
            return Result<TokenBalanceDto>.Success(dto);
        }
    }
}
