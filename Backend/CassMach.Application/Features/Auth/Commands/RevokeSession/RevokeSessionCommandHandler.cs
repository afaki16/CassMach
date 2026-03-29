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

namespace CassMach.Application.Features.Auth.Commands.RevokeSession
{
    public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, Result>
    {
        private readonly IAuthService _authService;

        public RevokeSessionCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
        {
            return await _authService.RevokeTokenAsync(request.RefreshToken, request.IpAddress, request.UserAgent, request.Reason);
        }
    }
} 
