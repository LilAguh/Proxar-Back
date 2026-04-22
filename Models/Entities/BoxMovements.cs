using Models.Enums;

namespace Models;

public class BoxMovement
{
    public Guid Id { get; set; }
    public int Number { get; set; } // Autoincremental for display
    
    // Foreign Keys
    public Guid AccountId { get; set; }
    public Guid? TicketId { get; set; }
    public Guid UserId { get; set; }
    
    // Properties
    public MovementType Type { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string Concept { get; set; } = string.Empty;
    public string? VoucherNumber { get; set; }
    public string? Observations { get; set; }
    
    // Timestamps
    public DateTime MovementDate { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Account Account { get; set; } = null!;
    public Ticket? Ticket { get; set; }
    public User User { get; set; } = null!;
}