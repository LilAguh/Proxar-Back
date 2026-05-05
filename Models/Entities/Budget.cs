using Models.Enums;

namespace Models;

public class Budget
{
    public Guid Id { get; set; }
    public byte[]? RowVersion { get; set; }
    public int Number { get; set; } // Autoincremental por empresa

    // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Foreign Keys
    public Guid TicketId { get; set; }
    public Guid ClientId { get; set; }
    public Guid CreatedById { get; set; }

    // Snapshot del cliente (para histórico)
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string? ClientCUIT { get; set; }
    public string? ClientEmail { get; set; }
    public string? ClientAddress { get; set; }

    // Items (se guardan en BudgetItems)
    public ICollection<BudgetItem> Items { get; set; } = new List<BudgetItem>();

    // Totales
    public decimal Subtotal { get; set; }
    public decimal IVAAmount { get; set; }
    public decimal Total { get; set; }
    public decimal Discount { get; set; }

    // Validez
    public int ValidDays { get; set; } = 15;
    public DateTime ValidUntil { get; set; }

    // Estado
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;

    // PDF
    public string? PdfUrl { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Soft delete
    public bool Active { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Navigation properties
    public Ticket Ticket { get; set; } = null!;
    public Client Client { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
