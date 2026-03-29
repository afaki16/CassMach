using CassMach.Application.Features.Users.Dtos;
using CassMach.Application.Features.Roles.Dtos;
using CassMach.Application.Features.Tenants.Dtos;
using CassMach.Application.Features.Permissions.Dtos;
using CassMach.Application.Common.Results;
using CassMach.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace CassMach.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<Result<UserListDto>>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }
} 
