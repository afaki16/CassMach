using System.Collections.Generic;
using System.Threading.Tasks;
using CassMach.Domain.Entities;

namespace CassMach.Domain.Common.Interfaces.Repositories;

public interface ITokenTransactionRepository : IRepository<TokenTransaction, int>
{
    Task<IEnumerable<TokenTransaction>> GetByUserIdPaged(int userId, int page, int pageSize);
    Task<int> GetCountByUserId(int userId);
}
