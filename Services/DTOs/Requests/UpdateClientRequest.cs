using Models.Enums;

namespace Services.DTOs.Requests;

public class UpdateClientRequest
{
    // Datos básicos
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; } // Dirección de contacto/entrega
    public string? Notes { get; set; }

    // Datos fiscales para facturación
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public IVACondition? IVA { get; set; }
    public string? FiscalAddress { get; set; }
    public string? FiscalCity { get; set; }
    public string? FiscalProvince { get; set; }
    public string? FiscalPostalCode { get; set; }
}