namespace Models;

public class Client
{
    public Guid Id { get; set; }

        // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;


    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Soft delete
    public bool Active { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    

    // Navigation properties
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}