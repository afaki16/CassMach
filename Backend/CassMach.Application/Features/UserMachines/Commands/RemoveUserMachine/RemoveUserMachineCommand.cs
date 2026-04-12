using CassMach.Application.Common.Results;
using MediatR;

namespace CassMach.Application.Features.UserMachines.Commands.RemoveUserMachine
{
    public class RemoveUserMachineCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
