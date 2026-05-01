using FluentValidation;
using Models.Enums;
using Services.DTOs.Requests;

namespace Services.Validators;

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        // Identificación
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la empresa es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.LegalName)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.LegalName))
            .WithMessage("La razón social no puede exceder 200 caracteres");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("La URL del logo no puede exceder 500 caracteres");

        RuleFor(x => x.Website)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("El sitio web no puede exceder 500 caracteres");

        // Datos fiscales
        RuleFor(x => x.CUIT)
            .Matches(@"^\d{2}-?\d{8}-?\d$").When(x => !string.IsNullOrEmpty(x.CUIT))
            .WithMessage("El CUIT debe tener formato XX-XXXXXXXX-X (11 dígitos)");

        RuleFor(x => x.IVA)
            .Must(BeValidIVACondition).When(x => !string.IsNullOrEmpty(x.IVA))
            .WithMessage("Condición de IVA inválida. Valores válidos: ResponsableInscripto, Monotributista, Exento, NoResponsable, ConsumidorFinal");

        RuleFor(x => x.IIBB)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.IIBB))
            .WithMessage("El número de IIBB no puede exceder 20 caracteres");

        RuleFor(x => x.FiscalAddress)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.FiscalAddress))
            .WithMessage("La dirección fiscal no puede exceder 500 caracteres");

        RuleFor(x => x.FiscalCity)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FiscalCity))
            .WithMessage("La ciudad fiscal no puede exceder 100 caracteres");

        RuleFor(x => x.FiscalProvince)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FiscalProvince))
            .WithMessage("La provincia fiscal no puede exceder 100 caracteres");

        RuleFor(x => x.FiscalPostalCode)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.FiscalPostalCode))
            .WithMessage("El código postal fiscal no puede exceder 20 caracteres");

        RuleFor(x => x.StartOfActivities)
            .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.StartOfActivities.HasValue)
            .WithMessage("La fecha de inicio de actividades no puede ser futura");

        RuleFor(x => x.DefaultSalesPoint)
            .GreaterThan(0).When(x => x.DefaultSalesPoint.HasValue)
            .WithMessage("El punto de venta debe ser mayor a 0");

        // Contacto
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Formato de email inválido")
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("El email no puede exceder 255 caracteres");

        RuleFor(x => x.Phone)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("El teléfono no puede exceder 50 caracteres");

        RuleFor(x => x.MobilePhone)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.MobilePhone))
            .WithMessage("El celular no puede exceder 50 caracteres");

        RuleFor(x => x.SupportEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.SupportEmail))
            .WithMessage("Formato de email de soporte inválido")
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.SupportEmail))
            .WithMessage("El email de soporte no puede exceder 255 caracteres");

        // Configuración regional
        RuleFor(x => x.Currency)
            .Length(3).When(x => !string.IsNullOrEmpty(x.Currency))
            .WithMessage("El código de moneda debe tener 3 caracteres (ISO 4217)");

        RuleFor(x => x.TimeZoneId)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.TimeZoneId))
            .WithMessage("El identificador de zona horaria no puede exceder 100 caracteres");

        RuleFor(x => x.Language)
            .MaximumLength(10).When(x => !string.IsNullOrEmpty(x.Language))
            .WithMessage("El código de idioma no puede exceder 10 caracteres");

        RuleFor(x => x.DateFormat)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.DateFormat))
            .WithMessage("El formato de fecha no puede exceder 20 caracteres");
    }

    private static bool BeValidIVACondition(string? iva)
    {
        if (string.IsNullOrWhiteSpace(iva)) return true;
        return Enum.TryParse<IVACondition>(iva, out _);
    }
}
