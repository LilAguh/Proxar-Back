namespace Services.DTOs.Responses;

public class BudgetItemDto
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal IVAPercentage { get; set; }
    public decimal Subtotal { get; set; }
    public decimal IVAAmount { get; set; }
    public decimal Total { get; set; }
}
