namespace DeepMatch.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public Guid CatalystAnswer1Id { get; set; } 
    public Guid CatalystAnswer2Id { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public Answer CatalystAnswer1 { get; set; } = null!;
    public Answer CatalystAnswer2 { get; set; } = null!;
    public List<Message> Messages { get; set; } = new();

    // Бизнес-метод
    public bool InvolvesUser(Guid userId)
    {
        return User1Id == userId || User2Id == userId;
    }

    public Guid GetOtherUserId(Guid userId)
    {
        if (User1Id == userId) return User2Id;
        if (User2Id == userId) return User1Id;
        throw new ArgumentException("Пользователь не участвует в этом мэтче");
    }
}
