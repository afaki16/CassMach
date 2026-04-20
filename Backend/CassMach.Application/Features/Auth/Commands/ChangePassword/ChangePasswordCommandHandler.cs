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
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IPasswordService _passwordService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordCommandHandler(
            IPasswordService passwordService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _passwordService = passwordService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, "User not authenticated"));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, "User not found"));

            var verifyResult = _passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
            if (!verifyResult.IsSuccess)
                return Result.Failure(verifyResult.Errors);

            if (!verifyResult.Value)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, "Current password is incorrect"));

            var strengthResult = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!strengthResult.IsSuccess)
                return Result.Failure(strengthResult.Errors);

            if (!strengthResult.Value)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, "New password does not meet strength requirements"));

            var hashResult = _passwordService.HashPassword(request.NewPassword);
            if (!hashResult.IsSuccess)
                return Result.Failure(hashResult.Errors);

            user.PasswordHash = hashResult.Value;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
} 
