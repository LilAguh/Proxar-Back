using Models.Enums;

namespace Services.DTOs.Requests;

public class CreateDirectBudgetRequest
{
    // Cliente
    public Guid ClientId { get; set; }

    // Datos del ticket a crear
    public string TicketTitle { get; set; } = string.Empty;
    public string? TicketDescription { get; set; }
    public TicketType TicketType { get; set; } = TicketType.Otro;
    public Priority TicketPriority { get; set; } = Priority.Intermedia;

    // Datos del presupuesto
    public int ValidDays { get; set; } = 15;
    public decimal Discount { get; set; } = 0;
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public List<CreateBudgetItemRequest> Items { get; set; } = new();
}
