using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IMapper _mapper;

    public TicketService(
        ITicketRepository ticketRepository,
        IClientRepository clientRepository,
        IUserRepository userRepository,
        ITicketHistoryRepository historyRepository,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
        _mapper = mapper;
    }

    public async Task<TicketDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId);
        if (ticket == null)
            throw new KeyNotFoundException("Ticket no encontrado");

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDetailDto> GetDetailsAsync(Guid id, Guid companyId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId);
        if (ticket == null)
            throw new KeyNotFoundException("Ticket no encontrado");

        var history = await _historyRepository.GetByTicketIdAsync(id);

        var dto = _mapper.Map<TicketDetailDto>(ticket);
        dto.History = _mapper.Map<List<TicketHistoryDto>>(history);

        return dto;
    }

    public async Task<IEnumerable<TicketDto>> GetAllByCompanyAsync(Guid companyId)
    {
        var tickets = await _ticketRepository.GetAllWithDetailsAsync(companyId);
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<IEnumerable<TicketDto>> GetByStatusAsync(TicketState status, Guid companyId)
    {
        var tickets = await _ticketRepository.GetByStatusAsync(status, companyId);
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<IEnumerable<TicketDto>> GetByClientAsync(Guid clientId, Guid companyId)
    {
        var tickets = await _ticketRepository.GetByClientAsync(clientId, companyId);
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<IEnumerable<TicketDto>> GetByAssignedUserAsync(Guid userId, Guid companyId)
    {
        var tickets = await _ticketRepository.GetByAssignedUserAsync(userId, companyId);
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketRequest request, Guid userId, Guid companyId)
    {
        // Verificar que el cliente existe y pertenece a la empresa
        var client = await _clientRepository.GetByIdAsync(request.ClientId, companyId);
        if (client == null)
            throw new KeyNotFoundException("Cliente no encontrado");

        // Verificar que el usuario asignado existe (si se especifica)
        if (request.AssignedToId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToId.Value, companyId);
            if (assignedUser == null)
                throw new KeyNotFoundException("Usuario asignado no encontrado");
        }

        var ticket = new Ticket
        {
            CompanyId = companyId,
            ClientId = request.ClientId,
            CreatedById = userId,
            AssignedToId = request.AssignedToId,
            Type = request.Type,
            Status = TicketState.Nuevo,
            Priority = request.Priority,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        var createdTicket = await _ticketRepository.AddAsync(ticket);

        // Crear historial
        var history = new TicketHistory
        {
            TicketId = createdTicket.Id,
            UserId = userId,
            Action = ActionHistorial.Creado,
            NewStatus = TicketState.Nuevo.ToString(),
            Timestamp = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history);

        return _mapper.Map<TicketDto>(createdTicket);
    }

    public async Task<TicketDto> UpdateTicketAsync(Guid id, UpdateTicketRequest request, Guid companyId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId);
        if (ticket == null)
            throw new KeyNotFoundException("Ticket no encontrado");

        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Address = request.Address;
        ticket.Priority = request.Priority;

        if (request.AssignedToId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToId.Value, companyId);
            if (assignedUser == null)
                throw new KeyNotFoundException("Usuario asignado no encontrado");
            
            ticket.AssignedToId = request.AssignedToId.Value;
        }

        await _ticketRepository.UpdateAsync(ticket);
        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDto> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusRequest request, Guid userId, Guid companyId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId);
        if (ticket == null)
            throw new KeyNotFoundException("Ticket no encontrado");

        var previousStatus = ticket.Status;
        ticket.Status = request.NewStatus;

        if (request.NewStatus == TicketState.Completado)
        {
            ticket.CompletedAt = DateTime.UtcNow;
        }

        await _ticketRepository.UpdateAsync(ticket);

        // Crear historial
        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            UserId = userId,
            Action = ActionHistorial.EstadoCambiado,
            PreviousStatus = previousStatus.ToString(),
            NewStatus = request.NewStatus.ToString(),
            Comment = request.Comment,
            Timestamp = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history);

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task SoftDeleteTicketAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        await _ticketRepository.SoftDeleteAsync(id, companyId, deletedBy);
    }
}