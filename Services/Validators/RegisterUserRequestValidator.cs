using FluentValidation;
using Services.DTOs.Requests;

namespace Services.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no es válido")
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .Matches("[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula")
            .Matches("[a-z]").WithMessage("La contraseña debe tener al menos una minúscula")
            .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe tener al menos un caracter especial");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Rol inválido");
    }
}