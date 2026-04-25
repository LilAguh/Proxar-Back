using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid companyId);
}