using Models.Enums;

namespace Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Ticket> TicketCreated { get; set; } = new List<Ticket>();
    public ICollection<Ticket> TicketAssigned { get; set; } = new List<Ticket>();
    public ICollection<TicketHistory> ActionHistory { get; set; } = new List<TicketHistory>();
    public ICollection<BoxMovement> Movements{ get; set; } = new List<BoxMovement>();
}
