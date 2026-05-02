using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Models.Enums;
using Services.DTOs.Responses;
using Services.Interfaces;
using Services.Utilities;

namespace Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IBoxMovementRepository _movementRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IMemoryCache _cache;

    public DashboardService(
        ITicketRepository ticketRepository,
        IBoxMovementRepository movementRepository,
        IAccountRepository accountRepository,
        ICompanyRepository companyRepository,
        IMapper mapper,
        IMemoryCache cache)
    {
        _ticketRepository = ticketRepository;
        _movementRepository = movementRepository;
        _accountRepository = accountRepository;
        _companyRepository = companyRepository;
        _cache = cache;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid companyId)
    {
        // NOTA: Cache simple sin invalidación automática.
        // Los datos pueden estar desactualizados hasta 30s después de:
        // - Crear/actualizar/eliminar tickets
        // - Registrar/eliminar movimientos de caja
        // - Modificar balances de cuentas
        // MVP: aceptable. Post-MVP: implementar invalidación o usar eventos.
        var cacheKey = $"dashboard:summary:{companyId}";
        if (_cache.TryGetValue(cacheKey, out DashboardSummaryDto? cachedSummary) && cachedSummary is not null)
        {
            return cachedSummary;
        }

        var tickets = await _ticketRepository.GetAllByCompanyAsync(companyId);
        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);

        var company = await _companyRepository.GetByIdAsync(companyId);
        var today = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company?.TimeZoneId);
        var (startUtc, endUtc) = BusinessDateTime.GetUtcRangeForBusinessDate(today, company?.TimeZoneId);
        var movements = await _movementRepository.GetByDateRangeAsync(
            startUtc, 
            endUtc, 
            companyId
        );

        var summary = new DashboardSummaryDto
        {
            Tickets = new TicketSummaryDto
            {
                Total = tickets.Count(),
                New = tickets.Count(t => t.Status == TicketState.Nuevo),
                InProgress = tickets.Count(t => 
                    t.Status == TicketState.EnVisita || 
                    t.Status == TicketState.EnProceso),
                Completed = tickets.Count(t => t.Status == TicketState.Completado)
            },
            CashToday = new CashSummaryDto
            {
                Income = movements.Where(m => m.Type == MovementType.Ingreso).Sum(m => m.Amount),
                Expense = movements.Where(m => m.Type == MovementType.Egreso).Sum(m => m.Amount),
                Net = movements.Where(m => m.Type == MovementType.Ingreso).Sum(m => m.Amount) -
                      movements.Where(m => m.Type == MovementType.Egreso).Sum(m => m.Amount)
            },
            TotalBalance = accounts.Sum(a => a.CurrentBalance),
            AccountBalances = accounts.ToDictionary(a => a.Id, a => a.CurrentBalance)
        };

        _cache.Set(cacheKey, summary, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            SlidingExpiration = TimeSpan.FromSeconds(15)
        });

        return summary;
    }
}
