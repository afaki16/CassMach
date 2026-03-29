using System.Collections.Generic;
using System.Threading.Tasks;
using CassMach.Domain.Entities;

namespace CassMach.Domain.Common.Interfaces.Repositories;

public interface ISystemSettingRepository
{
    Task<SystemSetting> GetByKey(string key);
    Task<List<SystemSetting>> GetAll();
    Task<SystemSetting> Upsert(string key, string value, int? updatedBy);
}
