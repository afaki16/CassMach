using CassMach.Application.Common.Results;
using MediatR;

namespace CassMach.Application.Features.Admin.Commands.RevokeUserSessions
{
    public class RevokeUserSessionsCommand : IRequest<Result>
    {
        public int UserId { get; set; }
        public string? Reason { get; set; }
    }
}
