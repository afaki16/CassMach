using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using MediatR;

namespace CassMach.Application.Features.Admin.Commands.UpdateSetting
{
    public class UpdateSettingCommand : IRequest<Result<SystemSettingDto>>
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int AdminUserId { get; set; }
    }
}
