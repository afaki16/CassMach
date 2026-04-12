using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using MediatR;

namespace CassMach.Application.Features.Machines.Commands.UpdateMachine
{
    public class UpdateMachineCommand : IRequest<Result<MachineDto>>
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
    }
}
