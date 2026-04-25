using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Models.Enums;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IBoxMovementRepository _movementRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public DashboardService(
        ITicketRepository ticketRepository,
        IBoxMovementRepository movementRepository,
        IAccountRepository accountRepository,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _movementRepository = movementRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid companyId)
    {
        var tickets = await _ticketRepository.GetAllByCompanyAsync(companyId);
        var accounts = await _accountRepository.GetActiveByCompanyAsync(companyId);
        
        var today = DateTime.UtcNow.Date;
        var movements = await _movementRepository.GetByDateRangeAsync(
            today, 
            today.AddDays(1).AddSeconds(-1), 
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

        return summary;
    }
}