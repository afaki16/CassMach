using CassMach.Application.Features.Users.Dtos;
using CassMach.Application.Features.Roles.Dtos;
using CassMach.Application.Features.Tenants.Dtos;
using CassMach.Application.Features.Permissions.Dtos;
using CassMach.Application.Common.Results;
using MediatR;

namespace CassMach.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<UserDto>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
} 
