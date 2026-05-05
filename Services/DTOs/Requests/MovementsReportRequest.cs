namespace Services.DTOs.Requests;

public class MovementsReportRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? TicketId { get; set; }
    public string? Type { get; set; } // "Ingreso", "Egreso"
    public string? PaymentMethod { get; set; } // "Efectivo", "Transferencia", etc.
}
