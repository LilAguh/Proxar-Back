using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class BoxMovementService : IBoxMovementService
{
    private readonly IBoxMovementRepository _movementRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;

    public BoxMovementService(
        IBoxMovementRepository movementRepository,
        IAccountRepository accountRepository,
        ITicketRepository ticketRepository,
        IMapper mapper)
    {
        _movementRepository = movementRepository;
        _accountRepository = accountRepository;
        _ticketRepository = ticketRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BoxMovementDto>> GetAllMovementsAsync()
    {
        var movements = await _movementRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<BoxMovementDto?> GetMovementByIdAsync(Guid id)
    {
        var movement = await _movementRepository.GetByIdAsync(id);
        return movement != null ? _mapper.Map<BoxMovementDto>(movement) : null;
    }

    public async Task<BoxMovementDto> RegisterMovementAsync(RegisterMovementRequest request, Guid userId)
    {
        // Verificar que la cuenta existe
        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
            throw new KeyNotFoundException($"Account with ID {request.AccountId} not found");

        // Verificar que el ticket existe (si se proporcionó)
        if (request.TicketId.HasValue)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId.Value);
            if (ticket == null)
                throw new KeyNotFoundException($"Ticket with ID {request.TicketId} not found");
        }

        // Validar saldo si es egreso
        if (request.Type == MovementType.Egreso && account.CurrentBalance < request.Amount)
        {
            throw new InvalidOperationException("Insufficient balance in account");
        }

        var movement = _mapper.Map<BoxMovement>(request);
        movement.UserId = userId;
        movement.RegisteredAt = DateTime.UtcNow;

        // Actualizar saldo de cuenta
        if (request.Type == MovementType.Ingreso)
            account.CurrentBalance += request.Amount;
        else
            account.CurrentBalance -= request.Amount;

        account.ModifiedAt = DateTime.UtcNow;

        // Guardar movimiento y actualizar cuenta
        var createdMovement = await _movementRepository.AddAsync(movement);
        await _accountRepository.UpdateAsync(account);

        return _mapper.Map<BoxMovementDto>(createdMovement);
    }

    public async Task<IEnumerable<BoxMovementDto>> GetMovementsByAccountAsync(Guid accountId)
    {
        var movements = await _movementRepository.GetByAccountAsync(accountId);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<IEnumerable<BoxMovementDto>> GetMovementsByTicketAsync(Guid ticketId)
    {
        var movements = await _movementRepository.GetByTicketAsync(ticketId);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<IEnumerable<BoxMovementDto>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var movements = await _movementRepository.GetByDateRangeAsync(startDate, endDate);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync()
    {
        var accounts = await _accountRepository.GetActiveAccountsAsync();
        return accounts.ToDictionary(a => a.Id, a => a.CurrentBalance);
    }
}