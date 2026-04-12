using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;

namespace CassMach.Application.Features.Machines.Queries.GetUserMachines
{
    public class GetUserMachinesQueryHandler : IRequestHandler<GetUserMachinesQuery, Result<IEnumerable<MachineDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserMachinesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<MachineDto>>> Handle(GetUserMachinesQuery request, CancellationToken cancellationToken)
        {
            var machines = await _unitOfWork.Machines.GetByUserId(request.UserId);
            return Result<IEnumerable<MachineDto>>.Success(_mapper.Map<IEnumerable<MachineDto>>(machines));
        }
    }
}
