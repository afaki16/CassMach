using CassMach.Application.Features.Users.Dtos;
using CassMach.Application.Features.Roles.Dtos;
using CassMach.Application.Features.Tenants.Dtos;
using CassMach.Application.Features.Permissions.Dtos;
using CassMach.Application.Common.Results;
using CassMach.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;   

namespace CassMach.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<Result<UserListDto>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public List<int> RoleIds { get; set; } = new List<int>();
        /// <summary>
        /// Optional. Only SuperAdmin can specify. If null, user is created in current user's tenant.
        /// </summary>
        public int? TenantId { get; set; }
    }
} 
