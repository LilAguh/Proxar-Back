using Models.Enums;

namespace Services.DTOs.Requests;

public class RegisterRequest
{
    // Datos del usuario administrador
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Datos básicos de la empresa
    public string CompanyName { get; set; } = string.Empty;
    public string CompanySlug { get; set; } = string.Empty;
    public string? LegalName { get; set; } // Razón social (si es diferente del nombre comercial)
    public string? LogoUrl { get; set; }

    // Datos fiscales obligatorios
    public string CUIT { get; set; } = string.Empty;
    public IVACondition IVA { get; set; }
    public string FiscalAddress { get; set; } = string.Empty;
    public string FiscalCity { get; set; } = string.Empty;
    public string FiscalProvince { get; set; } = string.Empty;
    public string FiscalPostalCode { get; set; } = string.Empty;
    public DateTime StartOfActivities { get; set; }

    // Datos fiscales opcionales
    public string? IIBB { get; set; }
    public int? DefaultSalesPoint { get; set; }

    // Contacto
    public string? Phone { get; set; }
    public string? CompanyEmail { get; set; }
}
