using CassMach.Domain.Entities;

namespace CassMach.Domain.Common.Interfaces.Repositories
{
    public interface IMachineRepository : IRepository<Machine, int>
    {
        Task<bool> ExistsByBrandAndModel(string brand, string model);
    }
}
