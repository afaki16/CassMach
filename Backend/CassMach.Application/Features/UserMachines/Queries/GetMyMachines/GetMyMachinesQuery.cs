using CassMach.Application.Common.Results;
using CassMach.Application.Features.UserMachines.Dtos;
using MediatR;

namespace CassMach.Application.Features.UserMachines.Queries.GetMyMachines
{
    public class GetMyMachinesQuery : IRequest<Result<IEnumerable<UserMachineDto>>>
    {
        public int UserId { get; set; }
    }
}
