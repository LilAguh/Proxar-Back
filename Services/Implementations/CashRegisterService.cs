using DataAccess.Repositories.Interfaces;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class CashRegisterService : ICashRegisterService
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBoxMovementRepository _boxMovementRepository;

    public CashRegisterService(
        ICashRegisterRepository cashRegisterRepository,
        IAccountRepository accountRepository,
        IBoxMovementRepository boxMovementRepository)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _accountRepository = accountRepository;
        _boxMovementRepository = boxMovementRepository;
    }

    public async Task<CashRegisterPreviewDto> GetOpenPreviewAsync(Guid companyId)
    {
        var today = DateTime.UtcNow.Date;
        var existing = await _cashRegisterRepository.GetTodayAsync(companyId);

        if (existing != null)
            return new CashRegisterPreviewDto { AlreadyOpenToday = true };

        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);
        var previous = await _cashRegisterRepository.GetPreviousClosedAsync(companyId, today);

        var preview = new CashRegisterPreviewDto
        {
            AlreadyOpenToday = false,
            Accounts = accounts.Select(a =>
            {
                var prevEntry = previous?.Entries.FirstOrDefault(e => e.AccountId == a.Id);
                return new AccountOpeningPreviewDto
                {
                    AccountId = a.Id,
                    AccountName = a.Name,
                    SuggestedAmount = prevEntry?.ClosingAmount
                };
            }).ToList()
        };

        // Calcular discrepancias: cierre anterior vs saldo actual de cuenta
        if (previous != null)
        {
            foreach (var prevEntry in previous.Entries)
            {
                var currentAccount = accounts.FirstOrDefault(a => a.Id == prevEntry.AccountId);
                if (currentAccount == null || prevEntry.ClosingAmount == null) continue;

                if (prevEntry.ClosingAmount != currentAccount.CurrentBalance)
                {
                    preview.Discrepancies.Add(new DiscrepancyDto
                    {
                        AccountId = prevEntry.AccountId,
                        AccountName = currentAccount.Name,
                        PreviousClosingAmount = prevEntry.ClosingAmount!.Value,
                        CurrentOpeningAmount = currentAccount.CurrentBalance,
                        Difference = currentAccount.CurrentBalance - prevEntry.ClosingAmount!.Value
                    });
                }
            }
        }

        return preview;
    }

    public async Task<CashRegisterDto> OpenAsync(OpenCashRegisterRequest request, Guid userId, Guid companyId)
    {
        var today = DateTime.UtcNow.Date;

        if (await _cashRegisterRepository.GetTodayAsync(companyId) != null)
            throw new InvalidOperationException("Ya existe una apertura de caja para hoy.");

        var register = new CashRegister
        {
            CompanyId = companyId,
            Date = today,
            Status = CashRegisterStatus.Open,
            OpenedAt = DateTime.UtcNow,
            OpenedById = userId,
            Notes = request.Notes,
            Entries = request.Entries.Select(e => new CashRegisterEntry
            {
                AccountId = e.AccountId,
                OpeningAmount = e.OpeningAmount
            }).ToList()
        };

        var created = await _cashRegisterRepository.AddAsync(register);
        return MapToDto(created, []);
    }

    public async Task<CashRegisterDto> CloseAsync(Guid registerId, CloseCashRegisterRequest request, Guid userId, Guid companyId)
    {
        var register = await _cashRegisterRepository.GetByIdAsync(registerId, companyId)
            ?? throw new KeyNotFoundException("Registro de caja no encontrado.");

        if (register.Status == CashRegisterStatus.Closed)
            throw new InvalidOperationException("Este registro ya fue cerrado.");

        foreach (var closeEntry in request.Entries)
        {
            var entry = register.Entries.FirstOrDefault(e => e.AccountId == closeEntry.AccountId);
            if (entry != null)
                entry.ClosingAmount = closeEntry.ClosingAmount;
        }

        register.Status = CashRegisterStatus.Closed;
        register.ClosedAt = DateTime.UtcNow;
        register.ClosedById = userId;
        if (!string.IsNullOrEmpty(request.Notes))
            register.Notes = request.Notes;

        await _cashRegisterRepository.UpdateAsync(register);
        return MapToDto(register, []);
    }

    public async Task<CashRegisterDto?> GetTodayAsync(Guid companyId)
    {
        var register = await _cashRegisterRepository.GetTodayAsync(companyId);
        return register == null ? null : MapToDto(register, []);
    }

    public async Task<CashRegisterDto?> GetByIdAsync(Guid id, Guid companyId)
    {
        var register = await _cashRegisterRepository.GetByIdAsync(id, companyId);
        if (register == null) return null;
        var movements = await _boxMovementRepository.GetByDateRangeAsync(register.Date, register.Date.AddDays(1).AddTicks(-1), companyId);
        return MapToDto(register, movements);
    }

    public async Task<IEnumerable<CashRegisterDto>> GetHistoryAsync(Guid companyId, int page, int pageSize)
    {
        var registers = await _cashRegisterRepository.GetHistoryAsync(companyId, page, pageSize);
        return registers.Select(r => MapToDto(r, []));
    }

    private static CashRegisterDto MapToDto(CashRegister r, IEnumerable<BoxMovement> movements) => new()
    {
        Id = r.Id,
        Date = r.Date,
        Status = r.Status,
        OpenedAt = r.OpenedAt,
        OpenedByName = r.OpenedBy?.Name ?? string.Empty,
        ClosedAt = r.ClosedAt,
        ClosedByName = r.ClosedBy?.Name,
        Notes = r.Notes,
        Entries = r.Entries.Select(e => new CashRegisterEntryDto
        {
            AccountId = e.AccountId,
            AccountName = e.Account?.Name ?? string.Empty,
            OpeningAmount = e.OpeningAmount,
            ClosingAmount = e.ClosingAmount
        }).ToList(),
        Movements = movements.Select(m => new BoxMovementDto
        {
            Id = m.Id,
            Number = m.Number,
            Account = new AccountDto
            {
                Id = m.Account!.Id,
                Name = m.Account.Name,
                Type = m.Account.Type,
                CurrentBalance = m.Account.CurrentBalance,
                Active = m.Account.Active,
                CreatedAt = m.Account.CreatedAt
            },
            TicketNumber = m.Ticket?.Number,
            User = new UserDto
            {
                Id = m.User!.Id,
                CompanyId = m.User.CompanyId,
                Name = m.User.Name,
                Email = m.User.Email,
                Role = m.User.Role,
                Active = m.User.Active
            },
            Type = m.Type,
            Amount = m.Amount,
            Method = m.Method,
            Concept = m.Concept,
            VoucherNumber = m.VoucherNumber,
            Observations = m.Observations,
            MovementDate = m.MovementDate,
            RegisteredAt = m.RegisteredAt
        }).ToList()
    };
}
