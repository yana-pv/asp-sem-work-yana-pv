using DeepMatch.Domain.Constants;
using FluentValidation;

namespace DeepMatch.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный email");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязательно")
            .MinimumLength(BusinessRules.Users.UserNameMinLength)
            .WithMessage($"Имя должно быть не менее {BusinessRules.Users.UserNameMinLength} символов")
            .MaximumLength(BusinessRules.Users.UserNameMaxLength)
            .WithMessage($"Имя должно быть не более {BusinessRules.Users.UserNameMaxLength} символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(BusinessRules.Users.PasswordMinLength)
            .WithMessage($"Пароль должен быть не менее {BusinessRules.Users.PasswordMinLength} символов");

        RuleFor(x => x.Age)
            .InclusiveBetween(BusinessRules.Users.MinAge, BusinessRules.Users.MaxAge)
            .WithMessage($"Возраст должен быть от {BusinessRules.Users.MinAge} до {BusinessRules.Users.MaxAge} лет");
    }
}
