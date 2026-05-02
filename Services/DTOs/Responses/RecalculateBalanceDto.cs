namespace Services.DTOs.Responses;

public class RecalculateBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal CalculatedBalance { get; set; }
    public decimal Discrepancy { get; set; }
    public bool HasDiscrepancy => Discrepancy != 0;
    public bool WasCorrected { get; set; }
}
