using Hangfire;
using DeepMatch.Application.Features.Questions.Commands.AssignDailyQuestion;
using DeepMatch.Application.Features.Answers.Commands.AnalyzeAnswerTags;
using DeepMatch.Application.Features.Badges.Commands.AssignBadges;
using DeepMatch.Infrastructure.Constants;

namespace DeepMatch.Infrastructure.Hangfire;

public static class HangfireJobScheduler
{
    public static void ScheduleJobs(IRecurringJobManager recurringJobManager)
    {
        recurringJobManager.AddOrUpdate<MediatorHangfireBridge>(
            "assign-daily-question",
            bridge => bridge.Send(new AssignDailyQuestionCommand()),
            HangfireSchedules.AssignDailyQuestion);

        recurringJobManager.AddOrUpdate<MediatorHangfireBridge>(
            "analyze-answer-tags",
            bridge => bridge.Send(new AnalyzeUnprocessedAnswersCommand()),
            HangfireSchedules.AnalyzeAnswerTags);

        recurringJobManager.AddOrUpdate<MediatorHangfireBridge>(
            "assign-badges",
            bridge => bridge.Send(new AssignBadgesCommand()),
            HangfireSchedules.AssignBadges);
    }
}
