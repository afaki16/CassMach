using CassMach.Domain.Entities;

namespace CassMach.Domain.Common.Interfaces.Repositories
{
    public interface IMachineRepository : IRepository<Machine, int>
    {
        Task<IEnumerable<Machine>> GetByUserId(int userId);
        Task<Machine?> GetByIdAndUserId(int id, int userId);
    }
}
