using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Domain.Entities;
using CassMach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CassMach.Infrastructure.Repositories;

public class ErrorSolutionRepository : RepositoryBase<ErrorSolution, int>, IErrorSolutionRepository
{
    private readonly ApplicationDbContext _context;

    public ErrorSolutionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ErrorSolution> GetAcceptedSolution(string brand, string errorCode)
    {
        return await _context.Set<ErrorSolution>()
            .Where(e => e.Brand.ToLower() == brand.ToLower()
                     && e.ErrorCode == errorCode
                     && e.IsAccepted == true)
            .OrderByDescending(e => e.CreatedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ErrorSolution>> GetByConversationId(Guid conversationId, int userId)
    {
        return await _context.Set<ErrorSolution>()
            .Where(e => e.ConversationId == conversationId && e.UserId == userId)
            .OrderBy(e => e.AttemptNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<ErrorSolution>> GetUserHistoryPaged(int userId, int page, int pageSize, string searchTerm = null)
    {
        var query = _context.Set<ErrorSolution>()
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.Brand.ToLower().Contains(term) ||
                (e.ErrorCode != null && e.ErrorCode.ToLower().Contains(term)) ||
                e.UserQuestion.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(e => e.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUserHistoryCount(int userId, string searchTerm = null)
    {
        var query = _context.Set<ErrorSolution>()
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.Brand.ToLower().Contains(term) ||
                (e.ErrorCode != null && e.ErrorCode.ToLower().Contains(term)) ||
                e.UserQuestion.ToLower().Contains(term));
        }

        return await query.CountAsync();
    }
}
