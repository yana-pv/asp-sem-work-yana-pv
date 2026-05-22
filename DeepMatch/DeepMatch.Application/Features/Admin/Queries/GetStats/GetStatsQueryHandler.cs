using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.Application.Features.Admin.Queries.GetStats;

public class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, AdminStatsDto>
{
    private readonly IUserRepository _users;
    private readonly IQuestionRepository _questions;
    private readonly IAnswerRepository _answers;
    private readonly IMatchRepository _matches;
    private readonly IReportRepository _reports;

    public GetStatsQueryHandler(
        IUserRepository users,
        IQuestionRepository questions,
        IAnswerRepository answers,
        IMatchRepository matches,
        IReportRepository reports)
    {
        _users = users;
        _questions = questions;
        _answers = answers;
        _matches = matches;
        _reports = reports;
    }

    public async Task<AdminStatsDto> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        var usersCount = await _users.CountNonSystemUsersAsync(cancellationToken);
        var blockedUsersCount = await _users.CountBlockedNonSystemUsersAsync(cancellationToken);
        var activeUsersCount = usersCount - blockedUsersCount;
        var questionsCount = await _questions.CountActiveAsync(cancellationToken);
        var answersCount = await _answers.CountAsync(cancellationToken);
        var matchesCount = await _matches.CountAsync(cancellationToken);
        var reportsCount = await _reports.CountAsync(cancellationToken);

        return new AdminStatsDto(
            usersCount,
            activeUsersCount,
            blockedUsersCount,
            questionsCount,
            answersCount,
            matchesCount,
            reportsCount);
    }
}
