using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Admin.Queries.GetAllUsersAdmin
{
    public class GetAllUsersAdminQueryHandler : IRequestHandler<GetAllUsersAdminQuery, PagedResult<AdminUserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllUsersAdminQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<AdminUserDto>> Handle(GetAllUsersAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Users.GetQueryable()
                .GroupJoin(
                    _unitOfWork.UserTokenBalances.GetQueryable(),
                    u => u.Id,
                    tb => tb.UserId,
                    (u, balances) => new { User = u, Balances = balances })
                .SelectMany(
                    x => x.Balances.DefaultIfEmpty(),
                    (x, balance) => new { x.User, Balance = balance });

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(x =>
                    x.User.FirstName.ToLower().Contains(term) ||
                    x.User.LastName.ToLower().Contains(term) ||
                    x.User.Email.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = await query
                .OrderByDescending(x => x.User.CreatedDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AdminUserDto
                {
                    Id = x.User.Id,
                    FullName = x.User.FirstName + " " + x.User.LastName,
                    Email = x.User.Email,
                    TokenBalance = x.Balance != null ? x.Balance.Balance : 0,
                    TotalCreditsUsed = x.Balance != null ? x.Balance.TotalCreditsUsed : 0,
                    TotalRawTokensUsed = x.Balance != null ? x.Balance.TotalRawTokensUsed : 0
                })
                .ToListAsync(cancellationToken);

            return PagedResult<AdminUserDto>.Success(items, request.Page, totalPages, totalCount);
        }
    }
}
