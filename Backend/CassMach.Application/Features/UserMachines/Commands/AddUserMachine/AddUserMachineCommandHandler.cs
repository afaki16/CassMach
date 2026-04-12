using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.UserMachines.Dtos;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Entities;
using CassMach.Domain.Models;
using MediatR;

namespace CassMach.Application.Features.UserMachines.Commands.AddUserMachine
{
    public class AddUserMachineCommandHandler : IRequestHandler<AddUserMachineCommand, Result<UserMachineDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddUserMachineCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserMachineDto>> Handle(AddUserMachineCommand request, CancellationToken cancellationToken)
        {
            var machine = await _unitOfWork.Machines.GetByIdAsync(request.MachineId);
            if (machine == null)
                return Result<UserMachineDto>.Failure(Error.Failure(ErrorCode.NotFound, "Makine kataloğda bulunamadı"));

            var alreadyAdded = await _unitOfWork.UserMachines.ExistsByUserIdAndMachineId(request.UserId, request.MachineId);
            if (alreadyAdded)
                return Result<UserMachineDto>.Failure(Error.Failure(ErrorCode.AlreadyExists, "Bu makine zaten listenizde mevcut"));

            var userMachine = new UserMachine
            {
                UserId = request.UserId,
                MachineId = request.MachineId,
                Name = request.Name?.Trim()
            };

            await _unitOfWork.UserMachines.AddAsync(userMachine);
            await _unitOfWork.SaveChangesAsync();

            // Include Machine navigation for DTO mapping
            userMachine.Machine = machine;

            return Result<UserMachineDto>.Success(_mapper.Map<UserMachineDto>(userMachine));
        }
    }
}
