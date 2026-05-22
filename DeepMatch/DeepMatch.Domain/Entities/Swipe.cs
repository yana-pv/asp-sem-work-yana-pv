using DeepMatch.Domain.Enums;

namespace DeepMatch.Domain.Entities;

public class Swipe
{
    public Guid Id { get; set; }
    public Guid SwiperUserId { get; set; }
    public Guid TargetAnswerId { get; set; }
    public SwipeDirection Direction { get; set; }
    public DateTime SwipedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public User SwiperUser { get; set; } = null!;
    public Answer TargetAnswer { get; set; } = null!;
}
