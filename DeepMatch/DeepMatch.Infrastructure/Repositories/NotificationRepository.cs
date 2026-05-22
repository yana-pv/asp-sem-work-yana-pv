using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Notifications.Common;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<NotificationDto>> GetRecentByUserAsync(Guid userId, int take, CancellationToken cancellationToken)
    {
        return _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .Select(n => new NotificationDto(n.Id, n.Type, n.Title, n.Link, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Notification>> GetUnreadByUserAsync(Guid userId, Guid? notificationId, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead);

        if (notificationId.HasValue)
        {
            query = query.Where(n => n.Id == notificationId.Value);
        }

        return query.ToListAsync(cancellationToken);
    }

    public void Add(Notification notification)
    {
        _context.Notifications.Add(notification);
    }

    public void AddRange(IEnumerable<Notification> notifications)
    {
        _context.Notifications.AddRange(notifications);
    }
}
