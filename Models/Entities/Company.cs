using Models.Enums;

namespace Models;

public class Company
{
    // Identificación
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty; // URL-friendly: "aberturas-sagitario"
    public string Name { get; set; } = string.Empty; // Nombre comercial
    public string? LegalName { get; set; } // Razón social (puede ser igual a Name)
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Datos fiscales (Argentina)
    public string? CUIT { get; set; } // 20-12345678-9
    public IVACondition? IVA { get; set; }
    public string? IIBB { get; set; } // Ingresos Brutos
    public string? FiscalAddress { get; set; }
    public string? FiscalCity { get; set; }
    public string? FiscalProvince { get; set; }
    public string? FiscalPostalCode { get; set; }
    public DateTime? StartOfActivities { get; set; } // Fecha de inicio de actividades

    // Configuración AFIP
    public int? DefaultSalesPoint { get; set; } // Punto de venta por defecto
    public string? CertPath { get; set; } // Path al certificado AFIP
    public string? CertPassword { get; set; } // Password del certificado (encriptado)

    // Contacto
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? SupportEmail { get; set; }

    // Configuración regional
    public string Currency { get; set; } = "ARS"; // ISO 4217
    public string TimeZoneId { get; set; } = "America/Argentina/Buenos_Aires"; // IANA timezone
    public string Language { get; set; } = "es-AR";
    public string DateFormat { get; set; } = "dd/MM/yyyy";

    // Soft delete
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navegación
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<BoxMovement> BoxMovements { get; set; } = new List<BoxMovement>();
    public ICollection<CashRegister> CashRegisters { get; set; } = new List<CashRegister>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<SubscriptionPayment> SubscriptionPayments { get; set; } = new List<SubscriptionPayment>();

    // Futuras relaciones
    // public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    // public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}