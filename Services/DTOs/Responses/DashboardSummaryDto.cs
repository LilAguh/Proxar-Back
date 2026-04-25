namespace Services.DTOs.Responses;

public class DashboardSummaryDto
{
    public TicketSummaryDto Tickets { get; set; } = new();
    public CashSummaryDto CashToday { get; set; } = new();
    public decimal TotalBalance { get; set; }
    public Dictionary<Guid, decimal> AccountBalances { get; set; } = new();
}

public class TicketSummaryDto
{
    public int Total { get; set; }
    public int New { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
}

public class CashSummaryDto
{
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
}
