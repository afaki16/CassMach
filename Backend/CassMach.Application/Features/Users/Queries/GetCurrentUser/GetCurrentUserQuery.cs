using CassMach.Application.Features.Users.Dtos;
using CassMach.Application.Common.Results;
using MediatR;

namespace CassMach.Application.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<Result<UserDto>>
    {
        public int UserId { get; set; }
    }
}
