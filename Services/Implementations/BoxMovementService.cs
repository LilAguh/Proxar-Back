using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Models;
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

    public async Task<BoxMovementDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var movement = await _movementRepository.GetByIdAsync(id, companyId);
        if (movement == null)
            throw new KeyNotFoundException("Movimiento no encontrado");

        return _mapper.Map<BoxMovementDto>(movement);
    }

    public async Task<IEnumerable<BoxMovementDto>> GetAllByCompanyAsync(Guid companyId)
    {
        var movements = await _movementRepository.GetAllWithDetailsAsync(companyId);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<IEnumerable<BoxMovementDto>> GetByAccountAsync(Guid accountId, Guid companyId)
    {
        var movements = await _movementRepository.GetByAccountAsync(accountId, companyId);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<IEnumerable<BoxMovementDto>> GetByTicketAsync(Guid ticketId, Guid companyId)
    {
        var movements = await _movementRepository.GetByTicketAsync(ticketId, companyId);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<BoxMovementDto> RegisterMovementAsync(RegisterMovementRequest request, Guid userId, Guid companyId)
    {
        // Verificar que la cuenta existe
        var account = await _accountRepository.GetByIdAsync(request.AccountId, companyId);
        if (account == null)
            throw new KeyNotFoundException("Cuenta no encontrada");

        // Verificar que el ticket existe (si se especifica)
        if (request.TicketId.HasValue)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId.Value, companyId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket no encontrado");
        }

        var movement = new BoxMovement
        {
            CompanyId = companyId,
            AccountId = request.AccountId,
            TicketId = request.TicketId,
            UserId = userId,
            Type = request.Type,
            Amount = request.Amount,
            Method = request.Method,
            Concept = request.Concept,
            VoucherNumber = request.VoucherNumber,
            Observations = request.Observations,
            MovementDate = request.MovementDate,
            Active = true,
            RegisteredAt = DateTime.UtcNow
        };

        var createdMovement = await _movementRepository.AddAsync(movement);

        // Actualizar saldo de la cuenta
        if (movement.Type == Models.Enums.MovementType.Ingreso)
        {
            account.CurrentBalance += movement.Amount;
        }
        else
        {
            account.CurrentBalance -= movement.Amount;
        }
        await _accountRepository.UpdateAsync(account);

        return _mapper.Map<BoxMovementDto>(createdMovement);
    }

    public async Task SoftDeleteMovementAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        var movement = await _movementRepository.GetByIdAsync(id, companyId);
        if (movement == null)
            throw new KeyNotFoundException("Movimiento no encontrado");

        // Revertir saldo en la cuenta
        var account = await _accountRepository.GetByIdAsync(movement.AccountId, companyId);
        if (account != null)
        {
            if (movement.Type == Models.Enums.MovementType.Ingreso)
            {
                account.CurrentBalance -= movement.Amount;
            }
            else
            {
                account.CurrentBalance += movement.Amount;
            }
            await _accountRepository.UpdateAsync(account);
        }

        await _movementRepository.SoftDeleteAsync(id, companyId, deletedBy);
    }
}