using CassMach.Application.Common.Results;
using CassMach.Application.Features.Tenants.Dtos;
using MediatR;
namespace CassMach.Application.Features.Tenants.Queries.GetTenantById
{
    public class GetTenantByIdQuery : IRequest<Result<TenantDto>>
    {
        public int Id { get; set; }
    }
}
