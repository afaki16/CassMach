using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Domain.Entities;
using CassMach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CassMach.Infrastructure.Repositories
{
    public class MachineRepository : RepositoryBase<Machine, int>, IMachineRepository
    {
        private readonly ApplicationDbContext _context;

        public MachineRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Machine>> GetByUserId(int userId)
        {
            return await _context.Set<Machine>()
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Brand)
                .ThenBy(m => m.Model)
                .ToListAsync();
        }

        public async Task<Machine?> GetByIdAndUserId(int id, int userId)
        {
            return await _context.Set<Machine>()
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }
    }
}
