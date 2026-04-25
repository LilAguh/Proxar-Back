using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface ICompanyService
{
    Task<CompanyDto> GetByIdAsync(Guid id);
    Task<CompanyDto> GetBySlugAsync(string slug);
    Task<IEnumerable<CompanyDto>> GetAllActiveAsync();
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request);
    Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request);
    Task DeactivateCompanyAsync(Guid id, Guid deletedBy);
}