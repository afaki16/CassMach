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

        public async Task<bool> ExistsByBrandAndModel(string brand, string model)
        {
            return await _context.Set<Machine>()
                .AnyAsync(m => m.Brand.ToLower() == brand.ToLower() && m.Model.ToLower() == model.ToLower());
        }
    }
}
