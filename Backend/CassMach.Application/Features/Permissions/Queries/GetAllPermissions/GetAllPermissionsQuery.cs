using CassMach.Application.Features.Users.Dtos;
using CassMach.Application.Features.Roles.Dtos;
using CassMach.Application.Features.Tenants.Dtos;
using CassMach.Application.Features.Permissions.Dtos;
using CassMach.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace CassMach.Application.Features.Permissions.Queries.GetAllPermissions
{
    public class GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>
    {
    }
} 
