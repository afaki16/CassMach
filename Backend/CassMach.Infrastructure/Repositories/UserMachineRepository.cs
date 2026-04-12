using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Domain.Entities;
using CassMach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CassMach.Infrastructure.Repositories
{
    public class UserMachineRepository : RepositoryBase<UserMachine, int>, IUserMachineRepository
    {
        private readonly ApplicationDbContext _context;

        public UserMachineRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<UserMachine>> GetByUserId(int userId)
        {
            return await _context.Set<UserMachine>()
                .Include(um => um.Machine)
                .Where(um => um.UserId == userId)
                .OrderBy(um => um.Machine.Brand)
                .ThenBy(um => um.Machine.Model)
                .ToListAsync();
        }

        public async Task<UserMachine?> GetByIdAndUserId(int id, int userId)
        {
            return await _context.Set<UserMachine>()
                .Include(um => um.Machine)
                .FirstOrDefaultAsync(um => um.Id == id && um.UserId == userId);
        }

        public async Task<bool> ExistsByUserIdAndMachineId(int userId, int machineId)
        {
            return await _context.Set<UserMachine>()
                .AnyAsync(um => um.UserId == userId && um.MachineId == machineId);
        }
    }
}
