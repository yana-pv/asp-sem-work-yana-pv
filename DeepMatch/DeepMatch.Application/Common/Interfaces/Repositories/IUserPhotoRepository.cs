using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IUserPhotoRepository
{
    Task<UserPhoto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(UserPhoto photo);
}
