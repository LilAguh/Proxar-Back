using FluentValidation;
using Services.DTOs.Requests;

namespace Services.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es requerida");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no puede superar los 100 caracteres")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial");

        RuleFor(x => x)
            .Must(x => x.CurrentPassword != x.NewPassword)
            .WithMessage("La nueva contraseña debe ser diferente a la actual")
            .When(x => !string.IsNullOrEmpty(x.CurrentPassword) && !string.IsNullOrEmpty(x.NewPassword));
    }
}
