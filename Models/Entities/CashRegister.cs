using Models.Enums;

namespace Models;

public class CashRegister
{
    public Guid Id { get; set; }
    public byte[]? RowVersion { get; set; } = [];

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Business date (timezone-aware). Stored as DATE in PostgreSQL.
    /// Always represents the business day in the company's timezone, not UTC.
    /// Example: If a company in Argentina (UTC-3) opens the register on May 2nd,
    /// this field stores "2026-05-02" regardless of UTC time.
    /// </summary>
    public DateOnly Date { get; set; }

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
