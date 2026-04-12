using CassMach.Application.Common.Results;
using CassMach.Application.Features.UserMachines.Dtos;
using MediatR;

namespace CassMach.Application.Features.UserMachines.Commands.AddUserMachine
{
    public class AddUserMachineCommand : IRequest<Result<UserMachineDto>>
    {
        public int UserId { get; set; }
        public int MachineId { get; set; }
        public string? Name { get; set; }
    }
}
