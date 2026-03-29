using CassMach.Application.Common.Results;
using MediatR;
using System;

namespace CassMach.Application.Features.Roles.Commands.DeleteRole
{
    public class DeleteRoleCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
