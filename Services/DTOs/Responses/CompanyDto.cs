namespace Services.DTOs.Responses;

public class CompanyDto
{
    // Identificación
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }

    // Datos fiscales
    public string? CUIT { get; set; }
    public string? IVA { get; set; }
    public string? IIBB { get; set; }
    public string? FiscalAddress { get; set; }
    public string? FiscalCity { get; set; }
    public string? FiscalProvince { get; set; }
    public string? FiscalPostalCode { get; set; }
    public DateTime? StartOfActivities { get; set; }
    public int? DefaultSalesPoint { get; set; }

    // Contacto
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? SupportEmail { get; set; }

    // Configuración regional
    public string Currency { get; set; } = "ARS";
    public string TimeZoneId { get; set; } = "America/Argentina/Buenos_Aires";
    public string Language { get; set; } = "es-AR";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
}