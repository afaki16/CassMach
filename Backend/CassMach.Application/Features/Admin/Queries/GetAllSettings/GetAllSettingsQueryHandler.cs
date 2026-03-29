using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Admin.Queries.GetAllSettings
{
    public class GetAllSettingsQueryHandler : IRequestHandler<GetAllSettingsQuery, Result<List<SystemSettingDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllSettingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<SystemSettingDto>>> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken)
        {
            var settings = await _unitOfWork.SystemSettings.GetAll();
            var dtos = _mapper.Map<List<SystemSettingDto>>(settings);
            return Result<List<SystemSettingDto>>.Success(dtos);
        }
    }
}
