using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Domain.Entities;
using CassMach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CassMach.Infrastructure.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly ApplicationDbContext _context;

    public SystemSettingRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SystemSetting> GetByKey(string key)
    {
        return await _context.Set<SystemSetting>()
            .FirstOrDefaultAsync(s => s.Key == key);
    }

    public async Task<List<SystemSetting>> GetAll()
    {
        return await _context.Set<SystemSetting>()
            .ToListAsync();
    }

    public async Task<SystemSetting> Upsert(string key, string value, int? updatedBy)
    {
        var setting = await _context.Set<SystemSetting>()
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Key = key,
                Value = value,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            };
            await _context.Set<SystemSetting>().AddAsync(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;
            _context.Set<SystemSetting>().Update(setting);
        }

        return setting;
    }
}
