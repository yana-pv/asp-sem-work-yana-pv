using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using AppValidationException = DeepMatch.Application.Common.Exceptions.ValidationException;

namespace DeepMatch.Application.Features.Reports.Commands.ReportUser;

public class ReportUserCommandHandler : IRequestHandler<ReportUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReportUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ReportUserCommand request, CancellationToken cancellationToken)
    {
        var reporterId = _currentUser.UserId;

        if (request.ReportedUserId == reporterId)
        {
            throw new AppValidationException(new List<ValidationFailure>
            {
                new("ReportedUserId", "Нельзя пожаловаться на самого себя")
            });
        }

        var reportedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ReportedUserId, cancellationToken);

        if (reportedUser == null)
        {
            throw new NotFoundException(nameof(User), request.ReportedUserId);
        }

        if (reportedUser.Role is UserRoles.Admin or UserRoles.System)
        {
            throw new ForbiddenException("Нельзя пожаловаться на этого пользователя");
        }

        var alreadyReported = await _context.Reports
            .AnyAsync(r => r.ReporterUserId == reporterId && r.ReportedUserId == request.ReportedUserId, cancellationToken);

        if (alreadyReported)
        {
            throw new AppValidationException(new List<ValidationFailure>
            {
                new("ReportedUserId", "Вы уже отправляли жалобу на этого пользователя")
            });
        }

        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterUserId = reporterId,
            ReportedUserId = request.ReportedUserId,
            Reason = request.Reason.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        reportedUser.ReportsCount++;

        if (reportedUser.ReportsCount >= BusinessRules.Reports.AutoBlockReportsCount && !reportedUser.IsBlocked)
        {
            reportedUser.IsBlocked = true;
            reportedUser.BlockReason = $"Автоматическая блокировка: {BusinessRules.Reports.AutoBlockReportsCount} жалоб";
            reportedUser.BlockedAt = DateTime.UtcNow;
        }

        _context.Reports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
