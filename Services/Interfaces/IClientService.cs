using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllClientsAsync();
    Task<ClientDto?> GetClientByIdAsync(Guid id);
    Task<ClientDto> CreateClientAsync(CreateClientRequest request);
    Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request);
    Task DeleteClientAsync(Guid id);
    Task<IEnumerable<ClientDto>> SearchClientsByNameAsync(string name);
}