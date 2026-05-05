using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IBoxMovementRepository _movementRepository;
    private readonly IMapper _mapper;

    public AccountService(
        IAccountRepository accountRepository,
        IBoxMovementRepository movementRepository,
        IMapper mapper)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _mapper = mapper;
    }

    public async Task<AccountDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var account = await _accountRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Account.NotFound);

        return _mapper.Map<AccountDto>(account);
    }

    public async Task<IEnumerable<AccountDto>> GetAllByCompanyAsync(Guid companyId)
    {
        var accounts = await _accountRepository.GetAllByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<IEnumerable<AccountDto>> GetActiveByCompanyAsync(Guid companyId)
    {
        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<Dictionary<Guid, decimal>> GetBalancesByCompanyAsync(Guid companyId)
    {
        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);
        return accounts.ToDictionary(a => a.Id, a => a.CurrentBalance);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, Guid companyId)
    {
        var account = new Account
        {
            CompanyId = companyId,
            Name = request.Name.Trim(),
            Type = request.Type,
            CurrentBalance = request.InitialBalance,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var created = await _accountRepository.AddAsync(account);
        return _mapper.Map<AccountDto>(created);
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request, Guid companyId)
    {
        var account = await _accountRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Account.NotFound);

        account.Name = request.Name.Trim();
        account.Type = request.Type;
        account.Active = request.Active;

        await _accountRepository.UpdateAsync(account);
        return _mapper.Map<AccountDto>(account);
    }

    public async Task DeleteAccountAsync(Guid id, Guid companyId, Guid deletedBy)
    {
        await _accountRepository.SoftDeleteAsync(id, companyId, deletedBy);
    }

    public async Task<RecalculateBalanceDto> RecalculateBalanceAsync(Guid id, Guid companyId, bool autoCorrect = false)
    {
        var account = await _accountRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Account.NotFound);

        // Obtener todos los movimientos activos de esta cuenta
        var movements = await _movementRepository.GetByAccountAsync(id, companyId);

        // Calcular saldo desde movimientos: Ingresos suman, Egresos restan
        var calculatedBalance = movements
            .Where(m => m.Active)
            .Sum(m => m.Type == MovementType.Ingreso ? m.Amount : -m.Amount);

        var discrepancy = account.CurrentBalance - calculatedBalance;
        var wasCorrected = false;

        // Si hay discrepancia y se solicita corrección automática
        if (autoCorrect && discrepancy != 0)
        {
            // Ajustar saldo usando actualización atómica
            var delta = calculatedBalance - account.CurrentBalance;
            await _accountRepository.UpdateBalanceAtomicAsync(id, companyId, delta);
            wasCorrected = true;
        }

        return new RecalculateBalanceDto
        {
            AccountId = account.Id,
            AccountName = account.Name,
            CurrentBalance = account.CurrentBalance,
            CalculatedBalance = calculatedBalance,
            Discrepancy = discrepancy,
            WasCorrected = wasCorrected
        };
    }
}
