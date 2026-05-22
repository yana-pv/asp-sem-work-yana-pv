namespace DeepMatch.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsIcebreaker { get; set; } = false;
    public bool IsRead { get; set; } = false;

    // Навигационные свойства
    public Match Match { get; set; } = null!;
    public User SenderUser { get; set; } = null!;
}
