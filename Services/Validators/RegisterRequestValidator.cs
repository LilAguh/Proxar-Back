using FluentValidation;
using Services.DTOs.Requests;

namespace Services.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no es válido")
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("El nombre de la empresa es requerido")
            .MinimumLength(2).WithMessage("El nombre de la empresa debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre de la empresa no puede exceder 100 caracteres");

        RuleFor(x => x.CompanySlug)
            .NotEmpty().WithMessage("El slug es requerido")
            .MinimumLength(2).WithMessage("El slug debe tener al menos 2 caracteres")
            .MaximumLength(50).WithMessage("El slug no puede exceder 50 caracteres")
            .Matches(@"^[a-z0-9-]+$").WithMessage("El slug solo puede contener letras minúsculas, números y guiones");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("La URL del logo no puede exceder 500 caracteres");
    }
}
