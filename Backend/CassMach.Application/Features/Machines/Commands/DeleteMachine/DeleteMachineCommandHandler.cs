using CassMach.Application.Common.Results;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Models;
using MediatR;

namespace CassMach.Application.Features.Machines.Commands.DeleteMachine
{
    public class DeleteMachineCommandHandler : IRequestHandler<DeleteMachineCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMachineCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteMachineCommand request, CancellationToken cancellationToken)
        {
            var machine = await _unitOfWork.Machines.GetByIdAndUserId(request.Id, request.UserId);

            if (machine == null)
                return Result<bool>.Failure(Error.Failure(ErrorCode.NotFound, "Makine bulunamadı"));

            _unitOfWork.Machines.SoftDelete(machine);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
