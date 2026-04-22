using Models.Enums;

namespace Models;

public class TicketHistory
{
    public Guid Id { get; set; }
    
    // Foreign Keys
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    
    // Properties
    public AccionHistorial Action { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Ticket Ticket { get; set; } = null!;
    public User User { get; set; } = null!;
}
