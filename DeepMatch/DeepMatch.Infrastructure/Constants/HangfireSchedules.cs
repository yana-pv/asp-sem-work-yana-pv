namespace DeepMatch.Infrastructure.Constants;

public static class HangfireSchedules
{
    public const string AssignDailyQuestion = "1 0 * * *";
    public const string AnalyzeAnswerTags = "*/5 * * * *";
    public const string AssignBadges = "0 * * * *";
}
