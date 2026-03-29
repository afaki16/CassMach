using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CassMach.Domain.Entities;

namespace CassMach.Domain.Common.Interfaces.Repositories;

public interface IErrorSolutionRepository : IRepository<ErrorSolution, int>
{
    Task<ErrorSolution> GetAcceptedSolution(string brand, string errorCode);
    Task<List<ErrorSolution>> GetByConversationId(Guid conversationId, int userId);
    Task<IEnumerable<ErrorSolution>> GetUserHistoryPaged(int userId, int page, int pageSize, string searchTerm = null);
    Task<int> GetUserHistoryCount(int userId, string searchTerm = null);
}
