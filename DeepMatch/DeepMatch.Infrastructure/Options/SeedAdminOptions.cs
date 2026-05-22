namespace DeepMatch.Infrastructure.Options;

public class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
