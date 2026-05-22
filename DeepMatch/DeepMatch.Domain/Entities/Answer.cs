namespace DeepMatch.Domain.Entities;

public class Answer
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Tags { get; set; } = new();  
    public int LikesCount { get; set; } = 0;

    // Навигационные свойства
    public User User { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public List<Swipe> Swipes { get; set; } = new();
}
