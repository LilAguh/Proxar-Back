using Models.Enums;

namespace Models;

public class User
{
    public Guid Id { get; set; }
    
    // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    
    // Soft delete
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation
    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<BoxMovement> BoxMovements { get; set; } = new List<BoxMovement>();
    public ICollection<TicketHistory> TicketHistories { get; set; } = new List<TicketHistory>();
}
