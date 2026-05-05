using Models.Enums;

namespace Services.DTOs.Responses;

public class CashRegisterDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public CashRegisterStatus Status { get; set; }
    public DateTime OpenedAt { get; set; }
    public string OpenedByName { get; set; } = string.Empty;
    public DateTime? ClosedAt { get; set; }
    public string? ClosedByName { get; set; }
    public string? Notes { get; set; }
    public List<CashRegisterEntryDto> Entries { get; set; } = new();
    public List<DiscrepancyDto> Discrepancies { get; set; } = new();
    public List<BoxMovementDto> Movements { get; set; } = new();
}

public class CashRegisterEntryDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningAmount { get; set; }
    public decimal? ClosingAmount { get; set; }
}

public class DiscrepancyDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal PreviousClosingAmount { get; set; }
    public decimal CurrentOpeningAmount { get; set; }
    public decimal Difference { get; set; }
}

public class CashRegisterPreviewDto
{
    public bool AlreadyOpenToday { get; set; }
    public List<AccountOpeningPreviewDto> Accounts { get; set; } = new();
    public List<DiscrepancyDto> Discrepancies { get; set; } = new();
}

public class AccountOpeningPreviewDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal? SuggestedAmount { get; set; }
}
