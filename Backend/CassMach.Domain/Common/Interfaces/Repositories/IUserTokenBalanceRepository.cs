using System.Threading.Tasks;
using CassMach.Domain.Entities;

namespace CassMach.Domain.Common.Interfaces.Repositories;

public interface IUserTokenBalanceRepository : IRepository<UserTokenBalance, int>
{
    Task<UserTokenBalance> GetByUserId(int userId);
}
