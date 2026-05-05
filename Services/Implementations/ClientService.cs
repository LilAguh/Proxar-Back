using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
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

    public async Task<ClientDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var client = await _clientRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Client.NotFound);

        return _mapper.Map<ClientDto>(client);
    }

    public async Task<IEnumerable<ClientDto>> GetAllByCompanyAsync(Guid companyId)
    {
        var clients = await _clientRepository.GetAllByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<IEnumerable<ClientDto>> GetActiveByCompanyAsync(Guid companyId)
    {
        var clients = await _clientRepository.GetActiveByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<IEnumerable<ClientDto>> SearchByNameAsync(Guid companyId, string name)
    {
        var clients = await _clientRepository.SearchByNameAsync(companyId, name);
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request, Guid companyId)
    {
        var client = new Client
        {
            CompanyId = companyId,
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Notes = request.Notes,

            // Datos fiscales
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            IVA = request.IVA,
            FiscalAddress = request.FiscalAddress,
            FiscalCity = request.FiscalCity,
            FiscalProvince = request.FiscalProvince,
            FiscalPostalCode = request.FiscalPostalCode,

            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var createdClient = await _clientRepository.AddAsync(client);
        return _mapper.Map<ClientDto>(createdClient);
    }

    public async Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request, Guid companyId)
    {
        var client = await _clientRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Client.NotFound);

        client.Name = request.Name;
        client.Phone = request.Phone;
        client.Email = request.Email;
        client.Address = request.Address;
        client.Notes = request.Notes;

        // Datos fiscales
        client.DocumentType = request.DocumentType;
        client.DocumentNumber = request.DocumentNumber;
        client.IVA = request.IVA;
        client.FiscalAddress = request.FiscalAddress;
        client.FiscalCity = request.FiscalCity;
        client.FiscalProvince = request.FiscalProvince;
        client.FiscalPostalCode = request.FiscalPostalCode;

        await _clientRepository.UpdateAsync(client);
        return _mapper.Map<ClientDto>(client);
    }

    public async Task SoftDeleteClientAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        await _clientRepository.SoftDeleteAsync(id, companyId, deletedBy);
    }
}
