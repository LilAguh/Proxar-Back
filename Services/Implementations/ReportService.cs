using AutoMapper;
using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
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
    private readonly ProxarDbContext _context;
    private readonly IMapper _mapper;

    public ReportService(
        ITicketRepository ticketRepository,
        IBoxMovementRepository movementRepository,
        IClientRepository clientRepository,
        IAccountRepository accountRepository,
        ICompanyRepository companyRepository,
        ProxarDbContext context,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _movementRepository = movementRepository;
        _clientRepository = clientRepository;
        _accountRepository = accountRepository;
        _companyRepository = companyRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<TicketsReportDto> GetTicketsReportAsync(TicketsReportRequest request, Guid companyId)
    {
        var query = _context.Tickets
            .IgnoreQueryFilters()
            .Include(t => t.Client)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.CompanyId == companyId);

        if (request.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= request.DateTo.Value);

        if (request.ClientId.HasValue)
            query = query.Where(t => t.ClientId == request.ClientId.Value);

        if (!string.IsNullOrWhiteSpace(request.State) && Enum.TryParse<TicketState>(request.State, out var ticketState))
            query = query.Where(t => t.Status == ticketState);

        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<TicketType>(request.Type, out var ticketType))
            query = query.Where(t => t.Type == ticketType);

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<Priority>(request.Priority, out var ticketPriority))
            query = query.Where(t => t.Priority == ticketPriority);

        if (request.AssignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == request.AssignedToId.Value);

        if (request.CreatedById.HasValue)
            query = query.Where(t => t.CreatedById == request.CreatedById.Value);

        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

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
        var movementQuery = _context.BoxMovements
            .IgnoreQueryFilters()
            .Include(m => m.Account)
            .Include(m => m.User)
            .Include(m => m.Ticket)
            .Where(m => m.CompanyId == companyId);

        if (request.DateFrom.HasValue)
            movementQuery = movementQuery.Where(m => m.MovementDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            movementQuery = movementQuery.Where(m => m.MovementDate <= request.DateTo.Value);

        if (request.AccountId.HasValue)
            movementQuery = movementQuery.Where(m => m.AccountId == request.AccountId.Value);

        if (request.TicketId.HasValue)
            movementQuery = movementQuery.Where(m => m.TicketId == request.TicketId.Value);

        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<MovementType>(request.Type, out var movementType))
            movementQuery = movementQuery.Where(m => m.Type == movementType);

        if (!string.IsNullOrWhiteSpace(request.PaymentMethod) && Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var paymentMethod))
            movementQuery = movementQuery.Where(m => m.Method == paymentMethod);

        var movements = await movementQuery.OrderByDescending(m => m.MovementDate).ToListAsync();

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
        var allTickets = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.CompanyId == companyId)
            .ToListAsync();
        var openTickets = allTickets.Count(t =>
            t.Status != TicketState.Completado && t.Status != TicketState.Descartado);
        var completedTickets = allTickets.Count(t => t.Status == TicketState.Completado);
        var discardedTickets = allTickets.Count(t => t.Status == TicketState.Descartado);

        // Finanzas mes actual
        var currentMonthMovements = await _context.BoxMovements
            .IgnoreQueryFilters()
            .Where(m => m.CompanyId == companyId && m.MovementDate >= currentMonthStartUtc)
            .ToListAsync();

        var currentMonthIncome = currentMonthMovements
            .Where(m => m.Type == MovementType.Ingreso)
            .Sum(m => m.Amount);

        var currentMonthExpense = currentMonthMovements
            .Where(m => m.Type == MovementType.Egreso)
            .Sum(m => m.Amount);

        // Finanzas mes anterior
        var previousMonthMovements = await _context.BoxMovements
            .IgnoreQueryFilters()
            .Where(m => m.CompanyId == companyId &&
                        m.MovementDate >= previousMonthStartUtc &&
                        m.MovementDate <= currentMonthStartUtc)
            .ToListAsync();

        var previousMonthIncome = previousMonthMovements
            .Where(m => m.Type == MovementType.Ingreso)
            .Sum(m => m.Amount);

        // Crecimiento
        var incomeGrowth = previousMonthIncome > 0
            ? ((currentMonthIncome - previousMonthIncome) / previousMonthIncome) * 100
            : 0;

        // Balance total de cuentas
        var accounts = await _context.Accounts
            .IgnoreQueryFilters()
            .Where(a => a.CompanyId == companyId)
            .ToListAsync();
        var totalBalance = accounts.Sum(a => a.CurrentBalance);

        // Clientes
        var allClients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.CompanyId == companyId)
            .ToListAsync();
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
