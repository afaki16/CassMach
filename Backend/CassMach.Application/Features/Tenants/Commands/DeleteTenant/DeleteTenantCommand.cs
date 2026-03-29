using CassMach.Application.Common.Results;
using MediatR;

namespace CassMach.Application.Features.Tenants.Commands.DeleteTenant;

    public class DeleteTenantCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
