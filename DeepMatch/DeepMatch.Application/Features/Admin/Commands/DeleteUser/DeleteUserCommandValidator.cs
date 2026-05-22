using DeepMatch.Domain.Constants;
using FluentValidation;

namespace DeepMatch.Application.Features.Admin.Commands.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь обязателен");

        RuleFor(x => x.Reason)
            .MaximumLength(BusinessRules.Reports.MaxReasonLength)
            .WithMessage($"Причина блокировки должна быть не длиннее {BusinessRules.Reports.MaxReasonLength} символов");
    }
}
