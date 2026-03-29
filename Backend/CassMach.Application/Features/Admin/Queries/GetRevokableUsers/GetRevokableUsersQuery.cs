using CassMach.Application.Features.Admin.Dtos;
using CassMach.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace CassMach.Application.Features.Admin.Queries.GetRevokableUsers
{
    public class GetRevokableUsersQuery : IRequest<Result<IEnumerable<RevokableUserDto>>>
    {
        /// <summary>
        /// Optional. SuperAdmin can filter by tenant. Admin uses own tenant.
        /// </summary>
        public int? TenantId { get; set; }
    }
}
