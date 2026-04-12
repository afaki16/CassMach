using CassMach.Domain.Entities;

namespace CassMach.Domain.Common.Interfaces.Repositories
{
    public interface IUserMachineRepository : IRepository<UserMachine, int>
    {
        Task<IEnumerable<UserMachine>> GetByUserId(int userId);
        Task<UserMachine?> GetByIdAndUserId(int id, int userId);
        Task<bool> ExistsByUserIdAndMachineId(int userId, int machineId);
    }
}
