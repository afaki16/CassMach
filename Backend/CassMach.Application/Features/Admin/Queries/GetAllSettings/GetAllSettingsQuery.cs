using CassMach.Application.Common.Results;
using CassMach.Application.Features.Admin.Dtos;
using MediatR;
using System.Collections.Generic;

namespace CassMach.Application.Features.Admin.Queries.GetAllSettings
{
    public class GetAllSettingsQuery : IRequest<Result<List<SystemSettingDto>>>
    {
    }
}
