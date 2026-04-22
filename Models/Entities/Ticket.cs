using Models.Enums;

namespace Models;

public class Ticket
{
    public Guid Id { get; set; }
    public int Number { get; set; } // Autoincremental for display
    
    // Foreign Keys
    public Guid ClientId { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? AssignedToId { get; set; }
    
    // Properties
    public TicketType Type { get; set; }
    public TicketState Status { get; set; }
    public Priority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    public ICollection<BoxMovement> Movements { get; set; } = new List<BoxMovement>();
}
