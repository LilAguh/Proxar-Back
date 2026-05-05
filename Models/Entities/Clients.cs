using Models.Enums;

namespace Models;

public class Client
{
    public Guid Id { get; set; }
    public byte[]? RowVersion { get; set; } = [];

    // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Datos básicos
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; } // Dirección de contacto/entrega
    public string? Notes { get; set; }

    // Datos fiscales para facturación
    public string? DocumentType { get; set; } // CUIT, CUIL, DNI, Pasaporte, etc.
    public string? DocumentNumber { get; set; } // Número de documento
    public IVACondition? IVA { get; set; } // Condición frente al IVA
    public string? FiscalAddress { get; set; } // Dirección fiscal completa
    public string? FiscalCity { get; set; }
    public string? FiscalProvince { get; set; }
    public string? FiscalPostalCode { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Soft delete
    public bool Active { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    

    // Navigation properties
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
