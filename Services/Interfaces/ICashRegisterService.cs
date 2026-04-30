using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface ICashRegisterService
{
    Task<CashRegisterPreviewDto> GetOpenPreviewAsync(Guid companyId);
    Task<CashRegisterDto> OpenAsync(OpenCashRegisterRequest request, Guid userId, Guid companyId);
    Task<CashRegisterDto> CloseAsync(Guid registerId, CloseCashRegisterRequest request, Guid userId, Guid companyId);
    Task<CashRegisterDto?> GetTodayAsync(Guid companyId);
    Task<CashRegisterDto?> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<CashRegisterDto>> GetHistoryAsync(Guid companyId, int page, int pageSize);
}
