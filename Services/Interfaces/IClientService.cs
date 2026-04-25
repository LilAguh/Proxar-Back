using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IClientService
{
    Task<ClientDto> GetByIdAsync(Guid id, Guid companyId);
    Task<IEnumerable<ClientDto>> GetAllByCompanyAsync(Guid companyId);
    Task<IEnumerable<ClientDto>> GetActiveByCompanyAsync(Guid companyId);
    Task<ClientDto> CreateClientAsync(CreateClientRequest request, Guid companyId);
    Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request, Guid companyId);
    Task SoftDeleteClientAsync(Guid id, Guid companyId, Guid deletedBy);
}