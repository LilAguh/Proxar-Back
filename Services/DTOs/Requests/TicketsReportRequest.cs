namespace Services.DTOs.Requests;

public class TicketsReportRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? ClientId { get; set; }
    public string? State { get; set; } // "Nuevo", "EnProceso", etc.
    public string? Type { get; set; } // "Medicion", "Reparacion", etc.
    public string? Priority { get; set; } // "Baja", "Media", "Alta", "Urgente"
    public Guid? AssignedToId { get; set; }
    public Guid? CreatedById { get; set; }
}
