using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Entities;
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
            var machine = new Machine
            {
                UserId = request.UserId,
                Brand = request.Brand.Trim(),
                Model = request.Model.Trim(),
                Name = request.Name?.Trim()
            };

            await _unitOfWork.Machines.AddAsync(machine);
            await _unitOfWork.SaveChangesAsync();

            return Result<MachineDto>.Success(_mapper.Map<MachineDto>(machine));
        }
    }
}
