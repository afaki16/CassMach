using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Machines.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;

namespace CassMach.Application.Features.Machines.Queries.GetUserMachines
{
    public class GetMachinesQueryHandler : IRequestHandler<GetMachinesQuery, Result<IEnumerable<MachineDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMachinesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<MachineDto>>> Handle(GetMachinesQuery request, CancellationToken cancellationToken)
        {
            var machines = await _unitOfWork.Machines.GetAllAsync();
            var ordered = machines.OrderBy(m => m.Brand).ThenBy(m => m.Model);
            return Result<IEnumerable<MachineDto>>.Success(_mapper.Map<IEnumerable<MachineDto>>(ordered));
        }
    }
}
