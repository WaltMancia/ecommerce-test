using FluentValidation;
using SecureShop.Application.DTOs;

namespace SecureShop.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).Matches("^[a-zA-ZñÑáéíóúÁÉÍÓÚ ]*$");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}