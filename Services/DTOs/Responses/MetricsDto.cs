namespace Services.DTOs.Responses;

public class MetricsDto
{
    // Tickets
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int DiscardedTickets { get; set; }
    public double ConversionRate { get; set; } // % completados vs total
    public double AverageDaysToComplete { get; set; }

    // Finanzas (mes actual)
    public decimal CurrentMonthIncome { get; set; }
    public decimal CurrentMonthExpense { get; set; }
    public decimal CurrentMonthNet { get; set; }

    // Comparación con mes anterior
    public decimal PreviousMonthIncome { get; set; }
    public decimal IncomeGrowth { get; set; } // % de crecimiento

    // Cuentas
    public decimal TotalBalance { get; set; }

    // Clientes
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; } // Con tickets en el mes
}
