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
    private readonly IMapper _mapper;

    public TicketService(
        ITicketRepository ticketRepository,
        IClientRepository clientRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
    {
        var tickets = await _ticketRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<TicketDto?> GetTicketByIdAsync(Guid id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
    }

    public async Task<TicketDetailDto?> GetTicketWithDetailsAsync(Guid id)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        return ticket != null ? _mapper.Map<TicketDetailDto>(ticket) : null;
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketRequest request, Guid createdByUserId)
    {
        // Verificar que el cliente existe
        var client = await _clientRepository.GetByIdAsync(request.ClientId);
        if (client == null)
            throw new KeyNotFoundException($"Client with ID {request.ClientId} not found");

        // Verificar que el usuario existe
        var user = await _userRepository.GetByIdAsync(createdByUserId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {createdByUserId} not found");

        var ticket = _mapper.Map<Ticket>(request);
        ticket.CreatedById = createdByUserId;
        ticket.AssignedToId = createdByUserId; // Auto-asignar al creador
        ticket.Status = TicketState.Nuevo;
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.LastUpdatedAt = DateTime.UtcNow;

        var createdTicket = await _ticketRepository.AddAsync(ticket);

        // Crear entrada en historial
        var history = new TicketHistory
        {
            TicketId = createdTicket.Id,
            UserId = createdByUserId,
            Action = ActionHistorial.Creado,
            NewStatus = TicketState.Nuevo.ToString(),
            Timestamp = DateTime.UtcNow
        };

        // Aquí necesitarías un repositorio de TicketHistory para guardarlo
        // Por ahora lo dejamos así

        return _mapper.Map<TicketDto>(createdTicket);
    }

    public async Task<TicketDto> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusRequest request, Guid userId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {id} not found");

        var previousStatus = ticket.Status;
        ticket.Status = request.NewStatus;
        ticket.LastUpdatedAt = DateTime.UtcNow;

        if (request.NewStatus == TicketState.Completado)
            ticket.CompletedAt = DateTime.UtcNow;

        await _ticketRepository.UpdateAsync(ticket);

        // Crear entrada en historial
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

        // Guardar historial (necesitarías repositorio)

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDto> AssignTicketAsync(Guid id, AssignTicketRequest request)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {id} not found");

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        ticket.AssignedToId = request.UserId;
        ticket.LastUpdatedAt = DateTime.UtcNow;

        await _ticketRepository.UpdateAsync(ticket);

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByStatusAsync(TicketState status)
    {
        var tickets = await _ticketRepository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByAssignedUserAsync(Guid userId)
    {
        var tickets = await _ticketRepository.GetByAssignedUserAsync(userId);
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByClientAsync(Guid clientId)
    {
        var tickets = await _ticketRepository.GetByClientAsync(clientId);
        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }
}