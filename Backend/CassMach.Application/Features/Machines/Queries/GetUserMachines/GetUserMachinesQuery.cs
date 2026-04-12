using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using MediatR;

namespace CassMach.Application.Features.Machines.Queries.GetUserMachines
{
    public class GetUserMachinesQuery : IRequest<Result<IEnumerable<MachineDto>>>
    {
        public int UserId { get; set; }
    }
}
