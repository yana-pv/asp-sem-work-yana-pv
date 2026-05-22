using DeepMatch.Domain.Enums;

namespace DeepMatch.Domain.Entities;

public class Badge
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BadgeType Type { get; set; }
    public string? IconUrl { get; set; }

    // Навигационные свойства
    public List<User> Users { get; set; } = new();
}
