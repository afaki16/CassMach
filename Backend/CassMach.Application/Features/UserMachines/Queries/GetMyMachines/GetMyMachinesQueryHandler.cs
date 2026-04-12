using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.UserMachines.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;

namespace CassMach.Application.Features.UserMachines.Queries.GetMyMachines
{
    public class GetMyMachinesQueryHandler : IRequestHandler<GetMyMachinesQuery, Result<IEnumerable<UserMachineDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMyMachinesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserMachineDto>>> Handle(GetMyMachinesQuery request, CancellationToken cancellationToken)
        {
            var userMachines = await _unitOfWork.UserMachines.GetByUserId(request.UserId);
            return Result<IEnumerable<UserMachineDto>>.Success(_mapper.Map<IEnumerable<UserMachineDto>>(userMachines));
        }
    }
}
