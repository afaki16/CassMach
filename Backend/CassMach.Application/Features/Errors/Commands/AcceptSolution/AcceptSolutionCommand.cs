using CassMach.Application.Common.Results;
using MediatR;
using System;

namespace CassMach.Application.Features.Errors.Commands.AcceptSolution
{
    public class AcceptSolutionCommand : IRequest<Result<bool>>
    {
        public Guid ConversationId { get; set; }
        public int AttemptNumber { get; set; }
        public int UserId { get; set; }
    }
}
