using Models.Enums;

namespace Models;

public class Account
{
    public Guid Id { get; set; }


     // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;


    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal CurrentBalance { get; set; } = 0;
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    
    // Soft delete
    // public bool Active { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Navigation properties
    public ICollection<BoxMovement> Movements { get; set; } = new List<BoxMovement>();
}