using FluentValidation;

namespace DeepMatch.Application.Features.Swipes.Commands.SwipeAnswer;

public class SwipeAnswerCommandValidator : AbstractValidator<SwipeAnswerCommand>
{
    public SwipeAnswerCommandValidator()
    {
        RuleFor(x => x.AnswerId)
            .NotEmpty().WithMessage("ID ответа обязателен");

        RuleFor(x => x.Direction)
            .Must(d => d == "like" || d == "pass")
            .WithMessage("Направление должно быть 'like' или 'pass'");
    }
}