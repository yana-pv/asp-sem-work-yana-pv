namespace DeepMatch.Domain.Constants;

public static class NotificationTypes
{
    public const string Badge = "badge";
    public const string Match = "match";
    public const string Message = "message";
    public const string Question = "question";
    public const string Rating = "rating";
}

public static class ClientRoutes
{
    public const string Daily = "/daily";
    public const string Matches = "/matches";
    public const string Profile = "/profile";

    public static string Match(Guid matchId) => $"/matches/{matchId}";
}
