namespace Models;

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL-friendly: "aberturas-sagitario"
    public string? LogoUrl { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<BoxMovement> BoxMovements { get; set; } = new List<BoxMovement>();
}