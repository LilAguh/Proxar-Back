using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
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
    private readonly IBoxMovementRepository _boxMovementRepository;
    private readonly IMapper _mapper;

    public TicketService(
        ITicketRepository ticketRepository,
        IClientRepository clientRepository,
        IUserRepository userRepository,
        ITicketHistoryRepository historyRepository,
        IBoxMovementRepository boxMovementRepository,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
        _boxMovementRepository = boxMovementRepository;
        _mapper = mapper;
    }

    public async Task<TicketDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Ticket.NotFound);

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDetailsDto> GetDetailsAsync(Guid id, Guid companyId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Ticket.NotFound);

        var historyTask = _historyRepository.GetByTicketIdAsync(id);
        var movementsTask = _boxMovementRepository.GetByTicketAsync(id, companyId);
        await Task.WhenAll(historyTask, movementsTask);

        var dto = _mapper.Map<TicketDetailsDto>(ticket);
        dto.History = _mapper.Map<List<TicketHistoryDto>>(historyTask.Result);
        dto.Movements = _mapper.Map<List<BoxMovementDto>>(movementsTask.Result);

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
        var client = await _clientRepository.GetByIdAsync(request.ClientId, companyId)
            ?? throw new NotFoundException(AppMessages.Client.NotFound);

        if (request.AssignedToId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToId.Value, companyId)
                ?? throw new NotFoundException(AppMessages.Ticket.AssignedUserNotFound);
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

        var history = new TicketHistory
        {
            CompanyId = companyId,
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
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Ticket.NotFound);

        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Address = request.Address;
        ticket.Priority = request.Priority;

        if (request.AssignedToId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToId.Value, companyId)
                ?? throw new NotFoundException(AppMessages.Ticket.AssignedUserNotFound);

            ticket.AssignedToId = request.AssignedToId.Value;
        }

        await _ticketRepository.UpdateAsync(ticket);
        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDto> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusRequest request, Guid userId, Guid companyId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Ticket.NotFound);

        var previousStatus = ticket.Status;
        ticket.Status = request.NewStatus;

        if (request.NewStatus == TicketState.Completado)
            ticket.CompletedAt = DateTime.UtcNow;

        await _ticketRepository.UpdateAsync(ticket);

        var history = new TicketHistory
        {
            CompanyId = companyId,
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
