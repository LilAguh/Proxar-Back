using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using Services.Utilities;

namespace Services.Implementations;

public class ReportService : IReportService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IBoxMovementRepository _movementRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;

    public ReportService(
        ITicketRepository ticketRepository,
        IBoxMovementRepository movementRepository,
        IClientRepository clientRepository,
        IAccountRepository accountRepository,
        ICompanyRepository companyRepository,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _movementRepository = movementRepository;
        _clientRepository = clientRepository;
        _accountRepository = accountRepository;
        _companyRepository = companyRepository;
        _mapper = mapper;
    }

    public async Task<TicketsReportDto> GetTicketsReportAsync(TicketsReportRequest request, Guid companyId)
    {
        var tickets = await _ticketRepository.GetFilteredAsync(
            companyId,
            request.DateFrom,
            request.DateTo,
            request.ClientId,
            request.State,
            request.Type,
            request.Priority,
            request.AssignedToId,
            request.CreatedById
        );

        var ticketDtos = _mapper.Map<List<TicketDto>>(tickets);

        var summary = new TicketsReportSummary
        {
            Total = tickets.Count,

            // Por estado
            ByStateNew = tickets.Count(t => t.Status == TicketState.Nuevo),
            ByStateInVisit = tickets.Count(t => t.Status == TicketState.EnVisita),
            ByStateBudgeted = tickets.Count(t => t.Status == TicketState.Presupuestado),
            ByStateApproved = tickets.Count(t => t.Status == TicketState.Aprobado),
            ByStateInProcess = tickets.Count(t => t.Status == TicketState.EnProceso),
            ByStateCompleted = tickets.Count(t => t.Status == TicketState.Completado),
            ByStateDiscarded = tickets.Count(t => t.Status == TicketState.Descartado),

            // Por tipo
            ByTypeRepair = tickets.Count(t => t.Type == TicketType.Reparacion),
            ByTypeMeasurement = tickets.Count(t => t.Type == TicketType.Medicion),
            ByTypeGlass = tickets.Count(t => t.Type == TicketType.Vidrio),
            ByTypeWindow = tickets.Count(t => t.Type == TicketType.Abertura),
            ByTypeConstruction = tickets.Count(t => t.Type == TicketType.Obra),
            ByTypeOther = tickets.Count(t => t.Type == TicketType.Otro),

            // Por prioridad
            ByPriorityLow = tickets.Count(t => t.Priority == Priority.Baja),
            ByPriorityMedium = tickets.Count(t => t.Priority == Priority.Intermedia),
            ByPriorityHigh = tickets.Count(t => t.Priority == Priority.Alta),
            ByPriorityUrgent = tickets.Count(t => t.Priority == Priority.Urgente),

            // Métricas
            AverageDaysToComplete = CalculateAverageDaysToComplete(tickets),
            ConversionRate = CalculateConversionRate(tickets)
        };

        return new TicketsReportDto
        {
            Tickets = ticketDtos,
            Summary = summary
        };
    }

    public async Task<MovementsReportDto> GetMovementsReportAsync(MovementsReportRequest request, Guid companyId)
    {
        var movements = await _movementRepository.GetFilteredAsync(
            companyId,
            request.DateFrom,
            request.DateTo,
            request.AccountId,
            request.TicketId,
            request.Type,
            request.PaymentMethod
        );

        var movementDtos = _mapper.Map<List<BoxMovementDto>>(movements);

        var totalIncome = movements.Where(m => m.Type == MovementType.Ingreso).Sum(m => m.Amount);
        var totalExpense = movements.Where(m => m.Type == MovementType.Egreso).Sum(m => m.Amount);

        // Resumen por cuenta
        var byAccount = movements
            .GroupBy(m => new { m.AccountId, m.Account.Name })
            .Select(g => new AccountSummary
            {
                AccountId = g.Key.AccountId,
                AccountName = g.Key.Name,
                Income = g.Where(m => m.Type == MovementType.Ingreso).Sum(m => m.Amount),
                Expense = g.Where(m => m.Type == MovementType.Egreso).Sum(m => m.Amount),
                Net = g.Where(m => m.Type == MovementType.Ingreso).Sum(m => m.Amount) -
                      g.Where(m => m.Type == MovementType.Egreso).Sum(m => m.Amount)
            })
            .ToList();

        // Resumen por medio de pago
        var byPaymentMethod = movements
            .GroupBy(m => m.Method)
            .Select(g => new PaymentMethodSummary
            {
                Method = g.Key.ToString(),
                Income = g.Where(m => m.Type == MovementType.Ingreso).Sum(m => m.Amount),
                Expense = g.Where(m => m.Type == MovementType.Egreso).Sum(m => m.Amount),
                Count = g.Count()
            })
            .ToList();

        var summary = new MovementsReportSummary
        {
            Total = movements.Count,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,
            ByAccount = byAccount,
            ByPaymentMethod = byPaymentMethod
        };

        return new MovementsReportDto
        {
            Movements = movementDtos,
            Summary = summary
        };
    }

    public async Task<MetricsDto> GetMetricsAsync(Guid companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        var businessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company?.TimeZoneId);
        var currentMonthStart = new DateTime(businessDate.Year, businessDate.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        // Convertir fechas de negocio a UTC para queries
        var currentMonthStartUtc = BusinessDateTime.ConvertBusinessDateToUtc(currentMonthStart, company?.TimeZoneId);
        var previousMonthStartUtc = BusinessDateTime.ConvertBusinessDateToUtc(previousMonthStart, company?.TimeZoneId);

        // Tickets totales
        var allTickets = await _ticketRepository.GetAllByCompanyAsync(companyId);
        var openTickets = allTickets.Count(t =>
            t.Status != TicketState.Completado && t.Status != TicketState.Descartado);
        var completedTickets = allTickets.Count(t => t.Status == TicketState.Completado);
        var discardedTickets = allTickets.Count(t => t.Status == TicketState.Descartado);

        // Finanzas mes actual
        var currentMonthMovements = await _movementRepository.GetFilteredAsync(
            companyId,
            currentMonthStartUtc,
            null,
            null, null, null, null
        );

        var currentMonthIncome = currentMonthMovements
            .Where(m => m.Type == MovementType.Ingreso)
            .Sum(m => m.Amount);

        var currentMonthExpense = currentMonthMovements
            .Where(m => m.Type == MovementType.Egreso)
            .Sum(m => m.Amount);

        // Finanzas mes anterior
        var previousMonthMovements = await _movementRepository.GetFilteredAsync(
            companyId,
            previousMonthStartUtc,
            currentMonthStartUtc,
            null, null, null, null
        );

        var previousMonthIncome = previousMonthMovements
            .Where(m => m.Type == MovementType.Ingreso)
            .Sum(m => m.Amount);

        // Crecimiento
        var incomeGrowth = previousMonthIncome > 0
            ? ((currentMonthIncome - previousMonthIncome) / previousMonthIncome) * 100
            : 0;

        // Balance total de cuentas
        var accounts = await _accountRepository.GetAllByCompanyAsync(companyId);
        var totalBalance = accounts.Sum(a => a.CurrentBalance);

        // Clientes
        var allClients = await _clientRepository.GetAllByCompanyAsync(companyId);
        var activeClients = allTickets
            .Where(t => t.CreatedAt >= currentMonthStart)
            .Select(t => t.ClientId)
            .Distinct()
            .Count();

        return new MetricsDto
        {
            TotalTickets = allTickets.Count(),
            OpenTickets = openTickets,
            CompletedTickets = completedTickets,
            DiscardedTickets = discardedTickets,
            ConversionRate = CalculateConversionRate(allTickets.ToList()),
            AverageDaysToComplete = CalculateAverageDaysToComplete(allTickets.ToList()),

            CurrentMonthIncome = currentMonthIncome,
            CurrentMonthExpense = currentMonthExpense,
            CurrentMonthNet = currentMonthIncome - currentMonthExpense,

            PreviousMonthIncome = previousMonthIncome,
            IncomeGrowth = incomeGrowth,

            TotalBalance = totalBalance,

            TotalClients = allClients.Count(),
            ActiveClients = activeClients
        };
    }

    private static double CalculateAverageDaysToComplete(List<Models.Ticket> tickets)
    {
        var completed = tickets.Where(t => t.Status == TicketState.Completado && t.CompletedAt.HasValue).ToList();
        if (!completed.Any()) return 0;

        var totalDays = completed.Sum(t => (t.CompletedAt!.Value - t.CreatedAt).TotalDays);
        return Math.Round(totalDays / completed.Count, 1);
    }

    private static double CalculateConversionRate(List<Models.Ticket> tickets)
    {
        if (!tickets.Any()) return 0;
        var completed = tickets.Count(t => t.Status == TicketState.Completado);
        return Math.Round((completed / (double)tickets.Count) * 100, 1);
    }
}
