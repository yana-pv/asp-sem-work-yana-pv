using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IBadgeRepository
{
    Task<List<Badge>> GetAllAsync(CancellationToken cancellationToken);
}
