using DataAccess.Repositories.Interfaces;
using Exceptions;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using Services.Utilities;

namespace Services.Implementations;

public class CashRegisterService : ICashRegisterService
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBoxMovementRepository _boxMovementRepository;
    private readonly ICompanyRepository _companyRepository;

    public CashRegisterService(
        ICashRegisterRepository cashRegisterRepository,
        IAccountRepository accountRepository,
        IBoxMovementRepository boxMovementRepository,
        ICompanyRepository companyRepository)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _accountRepository = accountRepository;
        _boxMovementRepository = boxMovementRepository;
        _companyRepository = companyRepository;
    }

    public async Task<CashRegisterPreviewDto> GetOpenPreviewAsync(Guid companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        var businessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company?.TimeZoneId);
        var utcDate = BusinessDateTime.ConvertBusinessDateToUtc(businessDate, company?.TimeZoneId);
        var existing = await _cashRegisterRepository.GetTodayAsync(companyId, utcDate);

        if (existing != null)
            return new CashRegisterPreviewDto { AlreadyOpenToday = true };

        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);
        var previous = await _cashRegisterRepository.GetPreviousClosedAsync(companyId, utcDate);

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
        var company = await _companyRepository.GetByIdAsync(companyId);
        var businessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company?.TimeZoneId);
        var utcDate = BusinessDateTime.ConvertBusinessDateToUtc(businessDate, company?.TimeZoneId);

        // Regla de flujo: solo puede existir una caja abierta por empresa
        var existingOpenRegister = await _cashRegisterRepository.GetOpenAsync(companyId);
        if (existingOpenRegister != null)
            throw new BusinessRuleException(AppMessages.CashRegister.AlreadyOpenToday);

        if (await _cashRegisterRepository.GetTodayAsync(companyId, utcDate) != null)
            throw new BusinessRuleException(AppMessages.CashRegister.AlreadyOpenToday);

        var register = new CashRegister
        {
            CompanyId = companyId,
            Date = utcDate,
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
        var movements = await GetMovementsForBusinessDateAsync(companyId, businessDate, company?.TimeZoneId);
        return MapToDto(created, movements);
    }

    public async Task<CashRegisterDto> CloseAsync(Guid registerId, CloseCashRegisterRequest request, Guid userId, Guid companyId)
    {
        var register = await _cashRegisterRepository.GetByIdAsync(registerId, companyId)
            ?? throw new NotFoundException(AppMessages.CashRegister.NotFound);

        var company = await _companyRepository.GetByIdAsync(companyId);
        var todayBusinessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company?.TimeZoneId);
        var todayBusinessDateUtc = BusinessDateTime.ConvertBusinessDateToUtc(todayBusinessDate, company?.TimeZoneId);

        if (register.Status == CashRegisterStatus.Closed)
            throw new BusinessRuleException(AppMessages.CashRegister.AlreadyClosed);

        // Regla de flujo: solo se puede cerrar la caja abierta del día actual
        if (register.Date.Date != todayBusinessDateUtc.Date)
            throw new BusinessRuleException(AppMessages.CashRegister.NotOpenForDate);

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
        var movements = await GetMovementsForBusinessDateAsync(companyId, register.Date, company?.TimeZoneId);
        return MapToDto(register, movements);
    }

    public async Task<CashRegisterDto?> GetTodayAsync(Guid companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        var businessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company?.TimeZoneId);
        var utcDate = BusinessDateTime.ConvertBusinessDateToUtc(businessDate, company?.TimeZoneId);
        var register = await _cashRegisterRepository.GetTodayAsync(companyId, utcDate);
        if (register == null) return null;

        var movements = await GetMovementsForBusinessDateAsync(companyId, businessDate, company?.TimeZoneId);
        return MapToDto(register, movements);
    }

    public async Task<CashRegisterDto?> GetByIdAsync(Guid id, Guid companyId)
    {
        var register = await _cashRegisterRepository.GetByIdAsync(id, companyId);
        if (register == null) return null;
        var company = await _companyRepository.GetByIdAsync(companyId);
        var (startUtc, endUtc) = BusinessDateTime.GetUtcRangeForBusinessDate(register.Date, company?.TimeZoneId);
        var movements = await _boxMovementRepository.GetByDateRangeAsync(startUtc, endUtc, companyId);
        return MapToDto(register, movements);
    }

    public async Task<IEnumerable<CashRegisterDto>> GetHistoryAsync(Guid companyId, int page, int pageSize)
    {
        var registers = await _cashRegisterRepository.GetHistoryAsync(companyId, page, pageSize);
        return registers.Select(r => MapToDto(r, []));
    }

    private async Task<IEnumerable<BoxMovement>> GetMovementsForBusinessDateAsync(Guid companyId, DateTime businessDate, string? timeZoneId)
    {
        var (startUtc, endUtc) = BusinessDateTime.GetUtcRangeForBusinessDate(businessDate, timeZoneId);
        return await _boxMovementRepository.GetByDateRangeAsync(startUtc, endUtc, companyId);
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
        Movements = movements
            .Where(m => m.Account != null && m.User != null)
            .Select(m => new BoxMovementDto
            {
                Id = m.Id,
                Number = m.Number,
                Account = new AccountDto
                {
                    Id = m.Account.Id,
                    Name = m.Account.Name,
                    Type = m.Account.Type,
                    CurrentBalance = m.Account.CurrentBalance,
                    Active = m.Account.Active,
                    CreatedAt = m.Account.CreatedAt
                },
                TicketNumber = m.Ticket?.Number,
                User = new UserDto
                {
                    Id = m.User.Id,
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
