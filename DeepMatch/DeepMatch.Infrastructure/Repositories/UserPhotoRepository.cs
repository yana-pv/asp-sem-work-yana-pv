using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class UserPhotoRepository : IUserPhotoRepository
{
    private readonly AppDbContext _context;

    public UserPhotoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<UserPhoto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.UserPhotos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public void Add(UserPhoto photo)
    {
        _context.UserPhotos.Add(photo);
    }
}
