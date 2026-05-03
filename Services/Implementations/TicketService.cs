using AutoMapper;
using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Exceptions;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using Services.Utilities;

namespace Services.Implementations;

public class TicketService : ITicketService
{
    private static readonly IReadOnlyDictionary<TicketState, TicketState[]> AllowedTransitions = new Dictionary<TicketState, TicketState[]>
    {
        [TicketState.Nuevo] = [TicketState.EnVisita, TicketState.Completado, TicketState.Descartado],
        [TicketState.EnVisita] = [TicketState.Presupuestado, TicketState.Completado, TicketState.Descartado],
        [TicketState.Presupuestado] = [TicketState.Aprobado, TicketState.Descartado],
        [TicketState.Aprobado] = [TicketState.EnProceso, TicketState.Completado, TicketState.Descartado],
        [TicketState.EnProceso] = [TicketState.Completado, TicketState.Descartado],
        [TicketState.Completado] = [],
        [TicketState.Descartado] = []
    };

    private readonly ITicketRepository _ticketRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IBoxMovementRepository _boxMovementRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ProxarDbContext _context;
    private readonly IMapper _mapper;

    public TicketService(
        ITicketRepository ticketRepository,
        IClientRepository clientRepository,
        IUserRepository userRepository,
        ITicketHistoryRepository historyRepository,
        IBoxMovementRepository boxMovementRepository,
        ICashRegisterRepository cashRegisterRepository,
        ICompanyRepository companyRepository,
        ProxarDbContext context,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
        _boxMovementRepository = boxMovementRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _companyRepository = companyRepository;
        _context = context;
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

        ValidateStatusTransition(ticket.Status, request.NewStatus);

        // VALIDACIÓN CRÍTICA: verificar que la caja esté abierta para cambiar estados de tickets
        // Esto garantiza que todo el flujo de trabajo del día esté dentro de una caja abierta
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        var businessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company.TimeZoneId);
        var businessDateOnly = DateOnly.FromDateTime(businessDate);
        var cashRegister = await _cashRegisterRepository.GetTodayAsync(companyId, businessDateOnly);

        if (cashRegister == null || cashRegister.Status != CashRegisterStatus.Open)
        {
            throw new BusinessRuleException(AppMessages.CashRegister.NotOpenForDate);
        }

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

    private static void ValidateStatusTransition(TicketState currentStatus, TicketState nextStatus)
    {
        if (currentStatus == nextStatus)
            throw new BusinessRuleException(AppMessages.Ticket.InvalidStatusTransition);

        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowedStates) || !allowedStates.Contains(nextStatus))
            throw new BusinessRuleException(AppMessages.Ticket.InvalidStatusTransition);
    }

    public async Task SoftDeleteTicketAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // CONSISTENCIA: Propagar soft delete a registros relacionados de forma atómica
            await _ticketRepository.SoftDeleteAsync(id, companyId, deletedBy);
            await _boxMovementRepository.SoftDeleteByTicketAsync(id, companyId, deletedBy);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
