using Models.Enums;

namespace Models;

public class TicketHistory
{
    public Guid Id { get; set; }

     // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    // Foreign Keys
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    
    // Properties
    public ActionHistorial Action { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;


    // Soft delete
    public bool Active { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }


    // Navigation properties
    public Ticket Ticket { get; set; } = null!;
    public User User { get; set; } = null!;
}
