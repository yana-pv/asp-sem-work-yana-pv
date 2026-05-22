using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class BadgeRepository : IBadgeRepository
{
    private readonly AppDbContext _context;

    public BadgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Badge>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _context.Badges.ToListAsync(cancellationToken);
    }
}
