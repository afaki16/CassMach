using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using MediatR;

namespace CassMach.Application.Features.Admin.Queries.GetDashboard
{
    public class GetDashboardQuery : IRequest<Result<DashboardDto>>
    {
    }
}
