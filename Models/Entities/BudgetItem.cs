namespace Models;

public class BudgetItem
{
    public Guid Id { get; set; }

    // Foreign Key
    public Guid BudgetId { get; set; }

    // Item properties
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal IVAPercentage { get; set; } = 21; // Default 21%

    // Calculated fields
    public decimal Subtotal { get; set; } // Quantity * UnitPrice
    public decimal IVAAmount { get; set; } // Subtotal * (IVAPercentage / 100)
    public decimal Total { get; set; } // Subtotal + IVAAmount

    // Navigation property
    public Budget Budget { get; set; } = null!;
}
