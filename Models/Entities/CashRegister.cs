using Models.Enums;

namespace Models;

public class CashRegister
{
    public Guid Id { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // El día al que corresponde esta apertura (solo fecha, sin hora)
    public DateTime Date { get; set; }

    public CashRegisterStatus Status { get; set; } = CashRegisterStatus.Open;

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public Guid OpenedById { get; set; }
    public User OpenedBy { get; set; } = null!;

    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedById { get; set; }
    public User? ClosedBy { get; set; }

    public string? Notes { get; set; }

    public ICollection<CashRegisterEntry> Entries { get; set; } = new List<CashRegisterEntry>();
}
