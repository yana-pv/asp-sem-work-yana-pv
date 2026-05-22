namespace DeepMatch.Domain.Entities;

public class Report
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public Guid ReportedUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User ReporterUser { get; set; } = null!;
    public User ReportedUser { get; set; } = null!;
}
