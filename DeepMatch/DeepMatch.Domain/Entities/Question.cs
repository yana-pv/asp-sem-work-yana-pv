using DeepMatch.Domain.Enums;

namespace DeepMatch.Domain.Entities;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionCategory Category { get; set; }
    public DateOnly? DateOfDay { get; set; }  
    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Навигационные свойства
    public List<Answer> Answers { get; set; } = new();
}
