namespace Models;

public class CashRegisterEntry
{
    public Guid Id { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Guid CashRegisterId { get; set; }
    public CashRegister CashRegister { get; set; } = null!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public decimal OpeningAmount { get; set; }
    public decimal? ClosingAmount { get; set; }
}
