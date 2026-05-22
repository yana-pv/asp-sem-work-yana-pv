using Microsoft.EntityFrameworkCore;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Question> Questions { get; }
    DbSet<Answer> Answers { get; }
    DbSet<Swipe> Swipes { get; }
    DbSet<Match> Matches { get; }
    DbSet<Message> Messages { get; }
    DbSet<Badge> Badges { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Report> Reports { get; }
    DbSet<UserPhoto> UserPhotos { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
