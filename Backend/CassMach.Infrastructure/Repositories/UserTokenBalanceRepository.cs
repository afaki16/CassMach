using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Domain.Entities;
using CassMach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CassMach.Infrastructure.Repositories;

public class UserTokenBalanceRepository : RepositoryBase<UserTokenBalance, int>, IUserTokenBalanceRepository
{
    private readonly ApplicationDbContext _context;

    public UserTokenBalanceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserTokenBalance> GetByUserId(int userId)
    {
        return await _context.Set<UserTokenBalance>()
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }
}
