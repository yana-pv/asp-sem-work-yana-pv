using DeepMatch.Domain.Constants;
using FluentValidation;

namespace DeepMatch.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Bio)
            .MaximumLength(BusinessRules.Users.BioMaxLength)
            .WithMessage($"Поле «О себе» должно быть не длиннее {BusinessRules.Users.BioMaxLength} символов");
    }
}
