using CassMach.Application.Common.Results;
using MediatR;

namespace CassMach.Application.Features.Machines.Commands.DeleteMachine
{
    public class DeleteMachineCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
}
