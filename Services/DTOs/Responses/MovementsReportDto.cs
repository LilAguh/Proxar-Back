namespace Services.DTOs.Responses;

public class MovementsReportDto
{
    public List<BoxMovementDto> Movements { get; set; } = new();
    public MovementsReportSummary Summary { get; set; } = new();
}

public class MovementsReportSummary
{
    public int Total { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }

    public List<AccountSummary> ByAccount { get; set; } = new();
    public List<PaymentMethodSummary> ByPaymentMethod { get; set; } = new();
}

public class AccountSummary
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
}

public class PaymentMethodSummary
{
    public string Method { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public int Count { get; set; }
}
