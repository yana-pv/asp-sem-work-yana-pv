using DeepMatch.Application.Features.Admin.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetProfileAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetPublicProfileAsync(Guid id, CancellationToken cancellationToken);
    Task<List<User>> GetUsersForBadgeAssignmentAsync(CancellationToken cancellationToken);
    Task<List<Guid>> GetActiveUserIdsAsync(CancellationToken cancellationToken);
    Task<List<AdminUserDto>> GetAdminUsersAsync(CancellationToken cancellationToken);
    Task<int> CountNonSystemUsersAsync(CancellationToken cancellationToken);
    Task<int> CountBlockedNonSystemUsersAsync(CancellationToken cancellationToken);
    void Add(User user);
}
