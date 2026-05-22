using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Admin.Common;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<User?> GetProfileAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Badges)
            .Include(u => u.Photos)
            .Include(u => u.Answers)
                .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetPublicProfileAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Badges)
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<List<User>> GetUsersForBadgeAssignmentAsync(CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Answers)
                .ThenInclude(a => a.Question)
            .Include(u => u.Badges)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Guid>> GetActiveUserIdsAsync(CancellationToken cancellationToken)
    {
        return _context.Users
            .Where(u => !u.IsBlocked)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<List<AdminUserDto>> GetAdminUsersAsync(CancellationToken cancellationToken)
    {
        return _context.Users
            .Where(u => u.Role != UserRoles.System)
            .OrderByDescending(u => u.RegisteredAt)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email,
                u.UserName,
                u.Role,
                u.Age,
                u.Rating.Value,
                u.ReportsCount,
                u.IsBlocked,
                u.BlockReason,
                u.RegisteredAt))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountNonSystemUsersAsync(CancellationToken cancellationToken)
    {
        return _context.Users.CountAsync(u => u.Role != UserRoles.System, cancellationToken);
    }

    public Task<int> CountBlockedNonSystemUsersAsync(CancellationToken cancellationToken)
    {
        return _context.Users.CountAsync(u => u.Role != UserRoles.System && u.IsBlocked, cancellationToken);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }
}
