using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Admin.Queries.GetDashboard
{
    public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDashboardQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            var totalUsers = await _unitOfWork.Users.GetQueryable().CountAsync(cancellationToken);

            var errorSolutionsQuery = _unitOfWork.ErrorSolutions.GetQueryable();

            var totalQuestions = await errorSolutionsQuery.CountAsync(cancellationToken);

            var totalAccepted = await errorSolutionsQuery
                .Where(e => e.IsAccepted == true)
                .CountAsync(cancellationToken);

            var totalCached = await errorSolutionsQuery
                .Where(e => e.FromCache)
                .CountAsync(cancellationToken);

            var balancesQuery = _unitOfWork.UserTokenBalances.GetQueryable();

            var totalTokensUsed = await balancesQuery
                .SumAsync(b => b.TotalRawTokensUsed, cancellationToken);

            var totalCreditsUsed = await balancesQuery
                .SumAsync(b => b.TotalCreditsUsed, cancellationToken);

            var dto = new DashboardDto
            {
                TotalUsers = totalUsers,
                TotalQuestions = totalQuestions,
                TotalAcceptedSolutions = totalAccepted,
                TotalCachedResponses = totalCached,
                TotalTokensUsed = totalTokensUsed,
                TotalCreditsUsed = totalCreditsUsed
            };

            return Result<DashboardDto>.Success(dto);
        }
    }
}
