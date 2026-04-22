using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Models;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IMapper _mapper;

    public ClientService(IClientRepository clientRepository, IMapper mapper)
    {
        _clientRepository = clientRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
    {
        var clients = await _clientRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<ClientDto?> GetClientByIdAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        return client != null ? _mapper.Map<ClientDto>(client) : null;
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request)
    {
        var client = _mapper.Map<Client>(request);
        client.CreatedAt = DateTime.UtcNow;
        client.ModifiedAt = DateTime.UtcNow;

        var createdClient = await _clientRepository.AddAsync(client);
        return _mapper.Map<ClientDto>(createdClient);
    }

    public async Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        
        if (client == null)
            throw new KeyNotFoundException($"Client with ID {id} not found");

        _mapper.Map(request, client);
        client.ModifiedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client);
        return _mapper.Map<ClientDto>(client);
    }

    public async Task DeleteClientAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        
        if (client == null)
            throw new KeyNotFoundException($"Client with ID {id} not found");

        await _clientRepository.DeleteAsync(client);
    }

    public async Task<IEnumerable<ClientDto>> SearchClientsByNameAsync(string name)
    {
        var clients = await _clientRepository.SearchByNameAsync(name);
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }
}