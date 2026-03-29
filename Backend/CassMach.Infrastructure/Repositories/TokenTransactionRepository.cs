using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Domain.Entities;
using CassMach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CassMach.Infrastructure.Repositories;

public class TokenTransactionRepository : RepositoryBase<TokenTransaction, int>, ITokenTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TokenTransactionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<TokenTransaction>> GetByUserIdPaged(int userId, int page, int pageSize)
    {
        return await _context.Set<TokenTransaction>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserId(int userId)
    {
        return await _context.Set<TokenTransaction>()
            .Where(t => t.UserId == userId)
            .CountAsync();
    }
}
