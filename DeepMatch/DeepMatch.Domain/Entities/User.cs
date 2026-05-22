using DeepMatch.Domain.Constants;
using DeepMatch.Domain.ValueObjects;

namespace DeepMatch.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.User;
    public int Age { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public Rating Rating { get; set; } = new(0);
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsBlocked { get; set; } = false;
    public int ReportsCount { get; set; } = 0;
    public string? BlockReason { get; set; }
    public DateTime? BlockedAt { get; set; }

    // Навигационные свойства
    public List<Badge> Badges { get; set; } = new();
    public List<Answer> Answers { get; set; } = new();
    public List<Swipe> Swipes { get; set; } = new();
    public List<Match> MatchesAsUser1 { get; set; } = new();
    public List<Match> MatchesAsUser2 { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
    public List<Report> ReportsFiled { get; set; } = new();
    public List<Report> ReportsReceived { get; set; } = new();
    public List<UserPhoto> Photos { get; set; } = new();

    // Бизнес-методы
    public int GetBadgeBonusSwipes()
    {
        return Badges.Count * BusinessRules.Swipes.PerBadgeBonus;
    }

    public int GetDailySwipeLimit()
    {
        return BusinessRules.Swipes.BaseDailyLimit + Rating.GetBonusSwipes() + GetBadgeBonusSwipes();
    }

    public bool CanSwipe(int swipesToday)
    {
        return swipesToday < GetDailySwipeLimit() && !IsBlocked;
    }
}
