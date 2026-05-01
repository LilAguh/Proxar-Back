using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IReportService
{
    Task<TicketsReportDto> GetTicketsReportAsync(TicketsReportRequest request, Guid companyId);
    Task<MovementsReportDto> GetMovementsReportAsync(MovementsReportRequest request, Guid companyId);
    Task<MetricsDto> GetMetricsAsync(Guid companyId);
}
