using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Entities;
using CassMach.Domain.Models;
using MediatR;

namespace CassMach.Application.Features.Machines.Commands.CreateMachine
{
    public class CreateMachineCommandHandler : IRequestHandler<CreateMachineCommand, Result<MachineDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateMachineCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<MachineDto>> Handle(CreateMachineCommand request, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.Machines.ExistsByBrandAndModel(request.Brand, request.Model);
            if (exists)
                return Result<MachineDto>.Failure(Error.Failure(ErrorCode.AlreadyExists, $"'{request.Brand} {request.Model}' kataloğda zaten mevcut"));

            var machine = new Machine
            {
                Brand = request.Brand.Trim(),
                Model = request.Model.Trim()
            };

            await _unitOfWork.Machines.AddAsync(machine);
            await _unitOfWork.SaveChangesAsync();

            return Result<MachineDto>.Success(_mapper.Map<MachineDto>(machine));
        }
    }
}
