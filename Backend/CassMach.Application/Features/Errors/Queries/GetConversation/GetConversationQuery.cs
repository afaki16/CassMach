using CassMach.Application.Common.Results;
using CassMach.Application.Features.Errors.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace CassMach.Application.Features.Errors.Queries.GetConversation
{
    public class GetConversationQuery : IRequest<Result<List<ErrorSolutionDto>>>
    {
        public Guid ConversationId { get; set; }
        public int UserId { get; set; }
    }
}
