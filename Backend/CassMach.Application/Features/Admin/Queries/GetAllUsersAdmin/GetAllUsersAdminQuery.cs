using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using MediatR;

namespace CassMach.Application.Features.Admin.Queries.GetAllUsersAdmin
{
    public class GetAllUsersAdminQuery : IRequest<PagedResult<AdminUserDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
    }
}
