using EssenceMvp.Application.Dtos;
using FluentValidation;

namespace EssenceMvp.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.DisplayName).MaximumLength(100).When(x => x.DisplayName is not null);
    }
}
