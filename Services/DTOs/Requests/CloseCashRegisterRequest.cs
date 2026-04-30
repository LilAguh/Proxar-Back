namespace Services.DTOs.Requests;

public class CloseCashRegisterRequest
{
    public List<CashRegisterCloseEntryRequest> Entries { get; set; } = new();
    public string? Notes { get; set; }
}

public class CashRegisterCloseEntryRequest
{
    public Guid AccountId { get; set; }
    public decimal ClosingAmount { get; set; }
}
