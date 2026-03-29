using CassMach.Application.Interfaces;
using CassMach.Application.Features.Auth.Commands.Login;
using CassMach.Application.Features.Auth.Commands.Logout;
using CassMach.Application.Features.Auth.Commands.LogoutAll;
using CassMach.Application.Features.Auth.Commands.LogoutDevice;
using CassMach.Application.Features.Auth.Commands.Register;
using CassMach.Application.Features.Auth.Commands.RefreshToken;
using CassMach.Application.Features.Auth.Commands.RevokeSession;
using CassMach.Application.Features.Auth.Commands.ChangePassword;
using CassMach.Application.Features.Auth.Commands.ForgotPassword;
using CassMach.Application.Features.Auth.Commands.ResetPassword;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Application.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Models;

namespace CassMach.Application.Features.Auth.Commands.LogoutDevice
{
    public class LogoutDeviceCommandHandler : IRequestHandler<LogoutDeviceCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public LogoutDeviceCommandHandler(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(LogoutDeviceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            return Result<int>.Failure(Error.Failure(
                   ErrorCode.NotFound,
                   "User not authenticated"));

        return await _authService.RevokeTokensByDeviceAsync(userId.Value, request.DeviceId, request.IpAddress, request.UserAgent, request.Reason);
        }
    }
} 
