using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Models;
using MediatR;

namespace CassMach.Application.Features.Machines.Commands.UpdateMachine
{
    public class UpdateMachineCommandHandler : IRequestHandler<UpdateMachineCommand, Result<MachineDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateMachineCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<MachineDto>> Handle(UpdateMachineCommand request, CancellationToken cancellationToken)
        {
            var machine = await _unitOfWork.Machines.GetByIdAndUserId(request.Id, request.UserId);

            if (machine == null)
                return Result<MachineDto>.Failure(Error.Failure(ErrorCode.NotFound, "Makine bulunamadı"));

            machine.Brand = request.Brand.Trim();
            machine.Model = request.Model.Trim();
            machine.Name = request.Name?.Trim();

            _unitOfWork.Machines.Update(machine);
            await _unitOfWork.SaveChangesAsync();

            return Result<MachineDto>.Success(_mapper.Map<MachineDto>(machine));
        }
    }
}
