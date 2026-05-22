using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.Application.Features.Admin.Queries.GetStats;

public class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, AdminStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminStatsDto> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        var usersCount = await _context.Users.CountAsync(u => u.Role != UserRoles.System, cancellationToken);
        var blockedUsersCount = await _context.Users.CountAsync(u => u.Role != UserRoles.System && u.IsBlocked, cancellationToken);
        var activeUsersCount = usersCount - blockedUsersCount;
        var questionsCount = await _context.Questions.CountAsync(q => q.IsActive, cancellationToken);
        var answersCount = await _context.Answers.CountAsync(cancellationToken);
        var matchesCount = await _context.Matches.CountAsync(cancellationToken);
        var reportsCount = await _context.Reports.CountAsync(cancellationToken);

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
