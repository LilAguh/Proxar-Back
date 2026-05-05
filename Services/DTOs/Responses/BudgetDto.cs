using Models.Enums;

namespace Services.DTOs.Responses;

public class BudgetDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public Guid TicketId { get; set; }
    public int TicketNumber { get; set; } // Calculado desde Ticket
    public Guid ClientId { get; set; }

    // Snapshot del cliente
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string? ClientCUIT { get; set; }
    public string? ClientEmail { get; set; }
    public string? ClientAddress { get; set; }

    // Items
    public List<BudgetItemDto> Items { get; set; } = new();

    // Totales
    public decimal Subtotal { get; set; }
    public decimal IVAAmount { get; set; }
    public decimal Total { get; set; }
    public decimal Discount { get; set; }

    // Validez
    public int ValidDays { get; set; }
    public DateTime ValidUntil { get; set; }

    // Estado
    public BudgetStatus Status { get; set; }

    // PDF
    public string? PdfUrl { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty; // Calculado desde User
}
