using CassMach.Application.Features.Users.Dtos;
using CassMach.Application.Features.Roles.Dtos;
using CassMach.Application.Features.Tenants.Dtos;
using CassMach.Application.Features.Permissions.Dtos;
using CassMach.Application.Common.Results;
using MediatR;
using System;

namespace CassMach.Application.Features.Roles.Queries.GetRoleById
{
    public class GetRoleByIdQuery : IRequest<Result<RoleDto>>
    {
        public int Id { get; set; }
    }
} 
