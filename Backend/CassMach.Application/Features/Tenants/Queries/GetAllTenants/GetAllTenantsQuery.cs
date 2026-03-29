using CassMach.Application.Common.Results;
using CassMach.Application.Features.Tenants.Dtos;
using MediatR;
using System.Collections.Generic;
namespace CassMach.Application.Features.Tenants.Queries.GetAllTenants
{
    public class GetAllTenantsQuery : IRequest<Result<IEnumerable<TenantListDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
    }
}
