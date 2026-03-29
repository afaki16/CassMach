using CassMach.Application.Common.Results;
using MediatR;
using System;

namespace CassMach.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
