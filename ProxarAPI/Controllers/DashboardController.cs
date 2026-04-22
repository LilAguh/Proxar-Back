using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using Services.Interfaces;

namespace ProxarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IBoxMovementService _movementService;

    public DashboardController(
        ITicketService ticketService,
        IBoxMovementService movementService)
    {
        _ticketService = ticketService;
        _movementService = movementService;
    }

    /// <summary>
    /// Get dashboard summary
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSummary()
    {
        var allTickets = await _ticketService.GetAllTicketsAsync();
        var newTickets = await _ticketService.GetTicketsByStatusAsync(TicketState.Nuevo);
        var inProgressTickets = await _ticketService.GetTicketsByStatusAsync(TicketState.EnProceso);
        var completedTickets = await _ticketService.GetTicketsByStatusAsync(TicketState.Completado);

        var today = DateTime.UtcNow.Date;
        var todayMovements = await _movementService.GetMovementsByDateRangeAsync(today, today.AddDays(1));

        var todayIncome = todayMovements
            .Where(m => m.Type == MovementType.Ingreso)
            .Sum(m => m.Amount);

        var todayExpense = todayMovements
            .Where(m => m.Type == MovementType.Egreso)
            .Sum(m => m.Amount);

        var balances = await _movementService.GetAccountBalancesAsync();

        var summary = new
        {
            Tickets = new
            {
                Total = allTickets.Count(),
                New = newTickets.Count(),
                InProgress = inProgressTickets.Count(),
                Completed = completedTickets.Count()
            },
            CashToday = new
            {
                Income = todayIncome,
                Expense = todayExpense,
                Net = todayIncome - todayExpense
            },
            AccountBalances = balances,
            TotalBalance = balances.Values.Sum()
        };

        return Ok(summary);
    }

    /// <summary>
    /// Get tickets by status count
    /// </summary>
    [HttpGet("tickets-by-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTicketsByStatus()
    {
        var statusCounts = new Dictionary<string, int>();

        foreach (TicketState status in Enum.GetValues(typeof(TicketState)))
        {
            var tickets = await _ticketService.GetTicketsByStatusAsync(status);
            statusCounts[status.ToString()] = tickets.Count();
        }

        return Ok(statusCounts);
    }
}