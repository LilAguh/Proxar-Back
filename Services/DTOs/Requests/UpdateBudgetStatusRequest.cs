using Models.Enums;

namespace Services.DTOs.Requests;

public class UpdateBudgetStatusRequest
{
    public BudgetStatus Status { get; set; }
}
