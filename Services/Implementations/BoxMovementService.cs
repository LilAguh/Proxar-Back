using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using Services.Utilities;

namespace Services.Implementations;

public class BoxMovementService : IBoxMovementService
{
    private readonly IBoxMovementRepository _movementRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IMapper _mapper;

    public BoxMovementService(
        IBoxMovementRepository movementRepository,
        IAccountRepository accountRepository,
        ITicketRepository ticketRepository,
        ICompanyRepository companyRepository,
        ICashRegisterRepository cashRegisterRepository,
        IMapper mapper)
    {
        _movementRepository = movementRepository;
        _accountRepository = accountRepository;
        _ticketRepository = ticketRepository;
        _companyRepository = companyRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _mapper = mapper;
    }

    public async Task<BoxMovementDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var movement = await _movementRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Movement.NotFound);

        return _mapper.Map<BoxMovementDto>(movement);
    }

    public async Task<IEnumerable<BoxMovementDto>> GetAllByCompanyAsync(Guid companyId)
    {
        var movements = await _movementRepository.GetAllWithDetailsAsync(companyId);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<PagedResultDto<BoxMovementDto>> GetPagedByCompanyAsync(Guid companyId, int page, int pageSize, MovementType? type = null)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        var (items, totalCount) = await _movementRepository.GetPagedByCompanyAsync(companyId, safePage, safePageSize, type);
        var mappedItems = _mapper.Map<List<BoxMovementDto>>(items);
        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);

        return new PagedResultDto<BoxMovementDto>
        {
            Items = mappedItems,
            Page = safePage,
            PageSize = safePageSize,
            TotalCount = totalCount,
            TotalPages = totalPages == 0 ? 1 : totalPages
        };
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

    public async Task<IEnumerable<BoxMovementDto>> GetByDateRangeAsync(DateTime from, DateTime to, Guid companyId)
    {
        var movements = await _movementRepository.GetByDateRangeAsync(from, to, companyId);
        return _mapper.Map<IEnumerable<BoxMovementDto>>(movements);
    }

    public async Task<BoxMovementDto> RegisterMovementAsync(RegisterMovementRequest request, Guid userId, Guid companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, companyId)
            ?? throw new NotFoundException(AppMessages.Account.NotFound);

        if (!account.Active)
            throw new BusinessRuleException(AppMessages.Account.Inactive);

        if (request.TicketId.HasValue)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId.Value, companyId)
                ?? throw new NotFoundException(AppMessages.Ticket.NotFound);
        }

        // Convertir fecha de negocio a UTC usando timezone de la empresa
        var movementDateUtc = BusinessDateTime.ConvertBusinessDateToUtc(request.MovementDate, company.TimeZoneId);

        // VALIDACIÓN CRÍTICA: siempre exigir caja abierta del día actual de la empresa
        var todayBusinessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company.TimeZoneId);
        var todayBusinessDateUtc = BusinessDateTime.ConvertBusinessDateToUtc(todayBusinessDate, company.TimeZoneId);
        var cashRegister = await _cashRegisterRepository.GetTodayAsync(companyId, todayBusinessDateUtc);

        if (cashRegister == null || cashRegister.Status != CashRegisterStatus.Open)
        {
            throw new BusinessRuleException(AppMessages.CashRegister.NotOpenForDate);
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
            MovementDate = movementDateUtc,
            Active = true,
            RegisteredAt = DateTime.UtcNow
        };

        var createdMovement = await _movementRepository.AddAsync(movement);

        // CONCURRENCIA: Actualización atómica del saldo para evitar condiciones de carrera
        var delta = movement.Type == Models.Enums.MovementType.Ingreso
            ? movement.Amount
            : -movement.Amount;
        await _accountRepository.UpdateBalanceAtomicAsync(account.Id, companyId, delta);

        return _mapper.Map<BoxMovementDto>(createdMovement);
    }

    public async Task SoftDeleteMovementAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        var movement = await _movementRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Movement.NotFound);

        // CONCURRENCIA: Revertir saldo de forma atómica
        // Ingreso se resta, Egreso se suma (operación inversa al registro)
        var delta = movement.Type == Models.Enums.MovementType.Ingreso
            ? -movement.Amount
            : movement.Amount;
        await _accountRepository.UpdateBalanceAtomicAsync(movement.AccountId, companyId, delta);

        await _movementRepository.SoftDeleteAsync(id, companyId, deletedBy);
    }
}
