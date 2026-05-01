namespace Services.DTOs.Responses;

public class TicketsReportDto
{
    public List<TicketDto> Tickets { get; set; } = new();
    public TicketsReportSummary Summary { get; set; } = new();
}

public class TicketsReportSummary
{
    public int Total { get; set; }
    public int ByStateNew { get; set; }
    public int ByStateInVisit { get; set; }
    public int ByStateBudgeted { get; set; }
    public int ByStateApproved { get; set; }
    public int ByStateInProcess { get; set; }
    public int ByStateCompleted { get; set; }
    public int ByStateDiscarded { get; set; }

    public int ByTypeRepair { get; set; }
    public int ByTypeMeasurement { get; set; }
    public int ByTypeGlass { get; set; }
    public int ByTypeWindow { get; set; }
    public int ByTypeConstruction { get; set; }
    public int ByTypeOther { get; set; }

    public int ByPriorityLow { get; set; }
    public int ByPriorityMedium { get; set; }
    public int ByPriorityHigh { get; set; }
    public int ByPriorityUrgent { get; set; }

    public double AverageDaysToComplete { get; set; }
    public double ConversionRate { get; set; } // % tickets completados
}
