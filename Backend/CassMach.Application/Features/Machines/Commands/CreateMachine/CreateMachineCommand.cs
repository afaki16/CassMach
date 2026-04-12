using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using MediatR;

namespace CassMach.Application.Features.Machines.Commands.CreateMachine
{
    public class CreateMachineCommand : IRequest<Result<MachineDto>>
    {
        public string Brand { get; set; }
        public string Model { get; set; }
    }
}
