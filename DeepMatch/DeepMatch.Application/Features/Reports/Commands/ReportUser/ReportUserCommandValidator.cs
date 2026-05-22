using DeepMatch.Domain.Constants;
using FluentValidation;

namespace DeepMatch.Application.Features.Reports.Commands.ReportUser;

public class ReportUserCommandValidator : AbstractValidator<ReportUserCommand>
{
    public ReportUserCommandValidator()
    {
        RuleFor(x => x.ReportedUserId)
            .NotEmpty().WithMessage("Пользователь для жалобы обязателен");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Укажите причину жалобы")
            .MinimumLength(BusinessRules.Reports.MinReasonLength)
            .WithMessage($"Причина жалобы должна быть не короче {BusinessRules.Reports.MinReasonLength} символов")
            .MaximumLength(BusinessRules.Reports.MaxReasonLength)
            .WithMessage($"Причина жалобы должна быть не длиннее {BusinessRules.Reports.MaxReasonLength} символов");
    }
}
