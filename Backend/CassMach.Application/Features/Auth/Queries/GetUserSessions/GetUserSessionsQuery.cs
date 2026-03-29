using CassMach.Application.Features.Auth.Dtos;
using CassMach.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace CassMach.Application.Features.Auth.Queries.GetUserSessions
{
    public class GetUserSessionsQuery : IRequest<Result<IEnumerable<SessionDto>>>
    {
        public int UserId { get; set; }
    }
} 
