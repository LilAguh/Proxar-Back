using FluentValidation;
using Services.DTOs.Requests;

namespace Services.Validators;

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la empresa es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("La URL del logo no puede exceder 500 caracteres");
    }
}
