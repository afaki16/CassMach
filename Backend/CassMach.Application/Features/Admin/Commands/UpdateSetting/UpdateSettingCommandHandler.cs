using AutoMapper;
using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using CassMach.Domain.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Features.Admin.Commands.UpdateSetting
{
    public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, Result<SystemSettingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateSettingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<SystemSettingDto>> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
        {
            var setting = await _unitOfWork.SystemSettings.Upsert(request.Key, request.Value, request.AdminUserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<SystemSettingDto>(setting);
            return Result<SystemSettingDto>.Success(dto);
        }
    }
}
