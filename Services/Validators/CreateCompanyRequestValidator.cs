using FluentValidation;
using Services.DTOs.Requests;

namespace Services.Validators;

public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la empresa es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("El slug es requerido")
            .MaximumLength(100).WithMessage("El slug no puede exceder 100 caracteres")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("El slug solo puede contener letras minúsculas, números y guiones");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("La URL del logo no puede exceder 500 caracteres");
    }
}
