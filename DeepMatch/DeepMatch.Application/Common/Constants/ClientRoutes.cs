namespace DeepMatch.Application.Common.Constants;

public static class ClientRoutes
{
    public const string Daily = "/daily";
    public const string Matches = "/matches";
    public const string Profile = "/profile";

    public static string Match(Guid matchId) => $"/matches/{matchId}";
}
