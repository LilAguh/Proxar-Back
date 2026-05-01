namespace Services.DTOs.Requests;

public class UpdateCompanyRequest
{
    // Identificación
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }

    // Datos fiscales
    public string? CUIT { get; set; }
    public string? IVA { get; set; } // "ResponsableInscripto", "Monotributista", etc.
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
    public string? Currency { get; set; }
    public string? TimeZoneId { get; set; }
    public string? Language { get; set; }
    public string? DateFormat { get; set; }
}