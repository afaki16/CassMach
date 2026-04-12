using CassMach.Application.Common.Results;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Models;
using MediatR;

namespace CassMach.Application.Features.UserMachines.Commands.RemoveUserMachine
{
    public class RemoveUserMachineCommandHandler : IRequestHandler<RemoveUserMachineCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveUserMachineCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(RemoveUserMachineCommand request, CancellationToken cancellationToken)
        {
            var userMachine = await _unitOfWork.UserMachines.GetByIdAndUserId(request.Id, request.UserId);

            if (userMachine == null)
                return Result<bool>.Failure(Error.Failure(ErrorCode.NotFound, "Makine listenizde bulunamadı"));

            _unitOfWork.UserMachines.SoftDelete(userMachine);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
