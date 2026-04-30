namespace Services.DTOs.Requests;

public class OpenCashRegisterRequest
{
    public List<CashRegisterEntryRequest> Entries { get; set; } = new();
    public string? Notes { get; set; }
}

public class CashRegisterEntryRequest
{
    public Guid AccountId { get; set; }
    public decimal OpeningAmount { get; set; }
}
