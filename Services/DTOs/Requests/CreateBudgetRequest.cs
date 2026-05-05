namespace Services.DTOs.Requests;

public class CreateBudgetRequest
{
    public Guid TicketId { get; set; }
    public int ValidDays { get; set; } = 15;
    public decimal Discount { get; set; } = 0;
    public List<CreateBudgetItemRequest> Items { get; set; } = new();
}

public class CreateBudgetItemRequest
{
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal IVAPercentage { get; set; } = 21;
}
