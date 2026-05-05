namespace Services.DTOs.Responses;

public class ClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }

    // Datos fiscales para facturación
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public int? IVA { get; set; }
    public string? FiscalAddress { get; set; }
    public string? FiscalCity { get; set; }
    public string? FiscalProvince { get; set; }
    public string? FiscalPostalCode { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}