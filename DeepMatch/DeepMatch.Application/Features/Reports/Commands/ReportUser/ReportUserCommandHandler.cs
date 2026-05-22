using FluentValidation.Results;
using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using AppValidationException = DeepMatch.Application.Common.Exceptions.ValidationException;

namespace DeepMatch.Application.Features.Reports.Commands.ReportUser;

public class ReportUserCommandHandler : IRequestHandler<ReportUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IReportRepository _reports;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ReportUserCommandHandler(
        IUserRepository users,
        IReportRepository reports,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _users = users;
        _reports = reports;
        _unitOfWork = unitOfWork;
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

        var reportedUser = await _users.GetByIdAsync(request.ReportedUserId, cancellationToken);

        if (reportedUser == null)
        {
            throw new NotFoundException(nameof(User), request.ReportedUserId);
        }

        if (reportedUser.Role is UserRoles.Admin or UserRoles.System)
        {
            throw new ForbiddenException("Нельзя пожаловаться на этого пользователя");
        }

        var alreadyReported = await _reports.ExistsByReporterAndReportedAsync(reporterId, request.ReportedUserId, cancellationToken);

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

        _reports.Add(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
