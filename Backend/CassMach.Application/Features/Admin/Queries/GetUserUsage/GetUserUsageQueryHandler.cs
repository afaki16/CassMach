using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Admin.Queries.GetUserUsage
{
    public class GetUserUsageQueryHandler : IRequestHandler<GetUserUsageQuery, Result<UserUsageReportDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserUsageQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserUsageReportDto>> Handle(GetUserUsageQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
                return Result<UserUsageReportDto>.Failure(Error.Failure(ErrorCode.NotFound, "User not found"));

            var balance = await _unitOfWork.UserTokenBalances.GetByUserId(request.UserId);

            var totalQuestions = await _unitOfWork.ErrorSolutions.GetUserHistoryCount(request.UserId);

            var acceptedCount = await _unitOfWork.ErrorSolutions.GetQueryable()
                .Where(e => e.UserId == request.UserId && e.IsAccepted == true)
                .CountAsync(cancellationToken);

            var recentTransactions = await _unitOfWork.TokenTransactions.GetByUserIdPaged(request.UserId, 1, 20);
            var transactionDtos = _mapper.Map<List<TokenTransactionDto>>(recentTransactions);

            var dto = new UserUsageReportDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CurrentBalance = balance?.Balance ?? 0,
                TotalRawTokensUsed = balance?.TotalRawTokensUsed ?? 0,
                TotalCreditsUsed = balance?.TotalCreditsUsed ?? 0,
                TotalQuestions = totalQuestions,
                TotalAcceptedSolutions = acceptedCount,
                RecentTransactions = transactionDtos
            };

            return Result<UserUsageReportDto>.Success(dto);
        }
    }
}
