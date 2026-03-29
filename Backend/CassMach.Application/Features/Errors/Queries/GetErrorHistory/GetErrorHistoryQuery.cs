using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using MediatR;

namespace CassMach.Application.Features.Errors.Queries.GetErrorHistory
{
    public class GetErrorHistoryQuery : IRequest<PagedResult<ErrorSolutionListDto>>
    {
        public int UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
    }
}
