using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using MediatR;

namespace CassMach.Application.Features.Admin.Queries.GetUserUsage
{
    public class GetUserUsageQuery : IRequest<Result<UserUsageReportDto>>
    {
        public int UserId { get; set; }
    }
}
