using AutoMapper;
using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using Services.Utilities;

namespace Services.Implementations;

public class BudgetService : IBudgetService
{
    private readonly ProxarDbContext _context;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketHistoryRepository _ticketHistoryRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IPdfService _pdfService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(
        ProxarDbContext context,
        IBudgetRepository budgetRepository,
        ITicketRepository ticketRepository,
        ITicketHistoryRepository ticketHistoryRepository,
        IClientRepository clientRepository,
        ICompanyRepository companyRepository,
        ICashRegisterRepository cashRegisterRepository,
        IPdfService pdfService,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<BudgetService> logger)
    {
        _context = context;
        _budgetRepository = budgetRepository;
        _ticketRepository = ticketRepository;
        _ticketHistoryRepository = ticketHistoryRepository;
        _clientRepository = clientRepository;
        _companyRepository = companyRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _pdfService = pdfService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BudgetDto> GetByIdAsync(Guid id, Guid companyId)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Budget.NotFound);

        return _mapper.Map<BudgetDto>(budget);
    }

    public async Task<IEnumerable<BudgetDto>> GetAllAsync(Guid companyId, int page = 1, int pageSize = 50)
    {
        var budgets = await _budgetRepository.GetAllAsync(companyId, page, pageSize);
        return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
    }

    public async Task<IEnumerable<BudgetDto>> GetByTicketIdAsync(Guid ticketId, Guid companyId, int page = 1, int pageSize = 50)
    {
        var budgets = await _budgetRepository.GetByTicketIdAsync(ticketId, companyId, page, pageSize);
        return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
    }

    public async Task<BudgetDto> CreateBudgetAsync(CreateBudgetRequest request, Guid userId, Guid companyId)
    {
        _logger.LogInformation("Iniciando creación de presupuesto para ticket {TicketId}, usuario {UserId}, empresa {CompanyId}",
            request.TicketId, userId, companyId);

        // Validar que el presupuesto tenga items
        if (request.Items == null || request.Items.Count == 0)
        {
            _logger.LogWarning("Intento de crear presupuesto sin items para ticket {TicketId}", request.TicketId);
            throw new BusinessRuleException(AppMessages.Budget.NoItems);
        }

        // Validar días de validez
        if (request.ValidDays <= 0 || request.ValidDays > 365)
        {
            throw new BusinessRuleException("La validez debe estar entre 1 y 365 días");
        }

        // Validar ticket
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, companyId)
            ?? throw new NotFoundException(AppMessages.Ticket.NotFound);

        // Validar cliente
        var client = await _clientRepository.GetByIdAsync(ticket.ClientId, companyId)
            ?? throw new NotFoundException(AppMessages.Client.NotFound);

        // Obtener datos de la empresa para el PDF
        var company = await GetCompanyAsync(companyId);

        // Obtener próximo número de presupuesto
        var nextNumber = await _budgetRepository.GetNextNumberAsync(companyId);

        // Calcular totales
        var calculation = CalculateBudgetTotals(request.Items, request.Discount);

        // Crear presupuesto
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Number = nextNumber,
            TicketId = ticket.Id,
            ClientId = client.Id,
            CreatedById = userId,

            // Snapshot del cliente
            ClientName = client.Name,
            ClientPhone = client.Phone,
            ClientCUIT = null, // TODO: Client.CUIT no existe aún en el modelo
            ClientEmail = client.Email,
            ClientAddress = client.Address,

            // Totales
            Subtotal = calculation.SubtotalAfterDiscount,
            IVAAmount = calculation.TotalIVA,
            Total = calculation.Total,
            Discount = request.Discount,

            // Validez
            ValidDays = request.ValidDays,
            ValidUntil = DateTime.UtcNow.AddDays(request.ValidDays),

            // Estado inicial
            Status = BudgetStatus.Draft,

            // Items
            Items = calculation.Items,

            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        // Usar transacción para garantizar atomicidad
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _logger.LogDebug("Iniciando transacción para presupuesto #{Number}, ticket {TicketId}", nextNumber, ticket.Id);

            // 1. Guardar presupuesto en BD
            await _budgetRepository.AddAsync(budget);

            // 2. Actualizar estado del ticket a Presupuestado
            if (ticket.Status != TicketState.Presupuestado)
            {
                var previousStatus = ticket.Status.ToString();
                ticket.Status = TicketState.Presupuestado;
                ticket.LastUpdatedAt = DateTime.UtcNow;
                await _ticketRepository.UpdateAsync(ticket);

                // 4. Registrar cambio en historial
                var history = new TicketHistory
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    TicketId = ticket.Id,
                    UserId = userId,
                    Action = ActionHistorial.EstadoCambiado,
                    PreviousStatus = previousStatus,
                    NewStatus = TicketState.Presupuestado.ToString(),
                    Comment = $"Presupuesto #{budget.Number} creado",
                    Timestamp = DateTime.UtcNow
                };
                await _ticketHistoryRepository.AddAsync(history);
            }

            // Confirmar transacción
            await transaction.CommitAsync();
            _logger.LogInformation("Presupuesto #{Number} creado exitosamente. Id: {BudgetId}, Total: {Total:C}",
                budget.Number, budget.Id, budget.Total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear presupuesto para ticket {TicketId}. Rollback ejecutado", request.TicketId);
            await transaction.RollbackAsync();
            throw;
        }

        // Recargar con relaciones para mapear
        var createdBudget = await _budgetRepository.GetByIdAsync(budget.Id, companyId);
        return _mapper.Map<BudgetDto>(createdBudget!);
    }

    public async Task<BudgetDto> CreateDirectBudgetAsync(CreateDirectBudgetRequest request, Guid userId, Guid companyId)
    {
        _logger.LogInformation("Iniciando creación de presupuesto directo para cliente {ClientId}, usuario {UserId}, empresa {CompanyId}",
            request.ClientId, userId, companyId);

        // Validar que el presupuesto tenga items
        if (request.Items == null || request.Items.Count == 0)
        {
            _logger.LogWarning("Intento de crear presupuesto directo sin items para cliente {ClientId}", request.ClientId);
            throw new BusinessRuleException(AppMessages.Budget.NoItems);
        }

        // Validar días de validez
        if (request.ValidDays <= 0 || request.ValidDays > 365)
        {
            throw new BusinessRuleException("La validez debe estar entre 1 y 365 días");
        }

        // Validar cliente
        var client = await _clientRepository.GetByIdAsync(request.ClientId, companyId)
            ?? throw new NotFoundException(AppMessages.Client.NotFound);

        // Obtener datos de la empresa para el PDF
        var company = await GetCompanyAsync(companyId);

        // Verificar caja abierta (regla de negocio crítica)
        var businessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company.TimeZoneId);
        var businessDateOnly = DateOnly.FromDateTime(businessDate);
        var cashRegister = await _cashRegisterRepository.GetTodayAsync(companyId, businessDateOnly);

        if (cashRegister == null || cashRegister.Status != CashRegisterStatus.Open)
        {
            _logger.LogWarning("Intento de crear presupuesto directo con caja cerrada. Empresa: {CompanyId}, Fecha: {BusinessDate}",
                companyId, businessDateOnly);
            throw new BusinessRuleException(AppMessages.CashRegister.NotOpenForDate);
        }

        // Crear ticket automáticamente en estado Presupuestado
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ClientId = client.Id,
            CreatedById = userId,
            AssignedToId = userId,
            Title = request.TicketTitle,
            Description = request.TicketDescription,
            Type = request.TicketType,
            Status = TicketState.Presupuestado,
            Priority = request.TicketPriority,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            Active = true
        };

        // Usar transacción para garantizar atomicidad
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _logger.LogDebug("Iniciando transacción para presupuesto directo, cliente {ClientId}", request.ClientId);

            // 1. Crear ticket
            await _ticketRepository.AddAsync(ticket);

            // Obtener próximo número de presupuesto
            var nextNumber = await _budgetRepository.GetNextNumberAsync(companyId);

            // Calcular totales
            var calculation = CalculateBudgetTotals(request.Items, request.Discount);

            // Crear presupuesto
            var budget = new Budget
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Number = nextNumber,
                TicketId = ticket.Id,
                ClientId = client.Id,
                CreatedById = userId,

                // Snapshot del cliente
                ClientName = client.Name,
                ClientPhone = client.Phone,
                ClientCUIT = null,
                ClientEmail = client.Email,
                ClientAddress = client.Address,

                // Totales
                Subtotal = calculation.SubtotalAfterDiscount,
                IVAAmount = calculation.TotalIVA,
                Total = calculation.Total,
                Discount = request.Discount,

                // Validez
                ValidDays = request.ValidDays,
                ValidUntil = DateTime.UtcNow.AddDays(request.ValidDays),

                // Estado inicial
                Status = BudgetStatus.Draft,

                // Items
                Items = calculation.Items,

                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            // 2. Guardar presupuesto en BD
            await _budgetRepository.AddAsync(budget);

            // 3. Registrar creación en historial
            var history = new TicketHistory
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                TicketId = ticket.Id,
                UserId = userId,
                Action = ActionHistorial.Creado,
                PreviousStatus = null,
                NewStatus = TicketState.Presupuestado.ToString(),
                Comment = $"Ticket creado con presupuesto #{budget.Number}",
                Timestamp = DateTime.UtcNow
            };
            await _ticketHistoryRepository.AddAsync(history);

            // Recargar con relaciones para mapear
            var createdBudget = await _budgetRepository.GetByIdAsync(budget.Id, companyId);

            // Confirmar transacción
            await transaction.CommitAsync();
            _logger.LogInformation("Presupuesto directo #{Number} y ticket {TicketId} creados exitosamente. Total: {Total:C}",
                budget.Number, ticket.Id, budget.Total);

            return _mapper.Map<BudgetDto>(createdBudget!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear presupuesto directo para cliente {ClientId}. Rollback ejecutado", request.ClientId);
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<BudgetDto> UpdateStatusAsync(Guid id, BudgetStatus status, Guid companyId)
    {
        _logger.LogInformation("Actualizando estado de presupuesto {BudgetId} a {Status}", id, status);

        var budget = await _budgetRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Budget.NotFound);

        var previousStatus = budget.Status;
        await _budgetRepository.UpdateStatusAsync(id, status, companyId);

        _logger.LogInformation("Presupuesto #{Number} cambió de estado: {PreviousStatus} → {NewStatus}",
            budget.Number, previousStatus, status);

        var updatedBudget = await _budgetRepository.GetByIdAsync(id, companyId);
        return _mapper.Map<BudgetDto>(updatedBudget!);
    }

    public async Task<byte[]> GetBudgetPdfAsync(Guid id, Guid companyId)
    {
        _logger.LogDebug("Generando PDF para presupuesto {BudgetId}", id);

        var budget = await _budgetRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Budget.NotFound);

        var company = await GetCompanyAsync(companyId);

        // Generar PDF en memoria (sin guardar en disco)
        return _pdfService.GenerateBudgetPdf(budget, company.Name);
    }

    private async Task<Company> GetCompanyAsync(Guid companyId)
    {
        var cacheKey = $"company_{companyId}";

        if (!_cache.TryGetValue(cacheKey, out Company? company))
        {
            company = await _companyRepository.GetByIdAsync(companyId)
                ?? throw new NotFoundException(AppMessages.Company.NotFound);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _cache.Set(cacheKey, company, cacheOptions);
        }

        return company!;
    }

    private record BudgetCalculation(
        List<BudgetItem> Items,
        decimal SubtotalAfterDiscount,
        decimal TotalIVA,
        decimal Total
    );

    private static BudgetCalculation CalculateBudgetTotals(List<CreateBudgetItemRequest> itemRequests, decimal discount)
    {
        // Validar que el presupuesto tenga items (ya validado antes, pero por seguridad)
        if (itemRequests == null || itemRequests.Count == 0)
        {
            throw new BusinessRuleException(AppMessages.Budget.NoItems);
        }

        var items = new List<BudgetItem>();
        decimal subtotalBeforeDiscount = 0;
        decimal totalIVA = 0;

        foreach (var itemRequest in itemRequests)
        {
            // Validar item
            if (itemRequest.Quantity <= 0)
                throw new BusinessRuleException("La cantidad debe ser mayor a 0");

            if (itemRequest.UnitPrice < 0)
                throw new BusinessRuleException("El precio unitario no puede ser negativo");

            if (string.IsNullOrWhiteSpace(itemRequest.Description))
                throw new BusinessRuleException("La descripción del item es requerida");

            if (!new[] { 0m, 10.5m, 21m, 27m }.Contains(itemRequest.IVAPercentage))
                throw new BusinessRuleException("Porcentaje de IVA inválido. Valores permitidos: 0%, 10.5%, 21%, 27%");

            var itemSubtotal = itemRequest.Quantity * itemRequest.UnitPrice;
            var itemIVA = itemSubtotal * (itemRequest.IVAPercentage / 100);
            var itemTotal = itemSubtotal + itemIVA;

            var budgetItem = new BudgetItem
            {
                Id = Guid.NewGuid(),
                Quantity = itemRequest.Quantity,
                Description = itemRequest.Description,
                UnitPrice = itemRequest.UnitPrice,
                IVAPercentage = itemRequest.IVAPercentage,
                Subtotal = itemSubtotal,
                IVAAmount = itemIVA,
                Total = itemTotal
            };

            items.Add(budgetItem);
            subtotalBeforeDiscount += itemSubtotal;
            totalIVA += itemIVA;
        }

        // Validar descuento
        if (discount < 0)
            throw new BusinessRuleException("El descuento no puede ser negativo");

        if (discount > subtotalBeforeDiscount)
            throw new BusinessRuleException("El descuento no puede ser mayor al subtotal");

        // Aplicar descuento al subtotal
        var subtotalAfterDiscount = subtotalBeforeDiscount - discount;

        // Recalcular IVA si hay descuento y actualizar items
        if (discount > 0 && subtotalBeforeDiscount > 0)
        {
            var discountRatio = discount / subtotalBeforeDiscount;
            totalIVA = 0;

            foreach (var item in items)
            {
                // Calcular subtotal con descuento
                var itemDiscountedSubtotal = item.Subtotal * (1 - discountRatio);

                // Calcular IVA sobre el subtotal descontado
                var itemIVA = itemDiscountedSubtotal * (item.IVAPercentage / 100);

                // Actualizar valores del item con descuento aplicado
                item.Subtotal = itemDiscountedSubtotal;
                item.IVAAmount = itemIVA;
                item.Total = itemDiscountedSubtotal + itemIVA;

                totalIVA += itemIVA;
            }
        }

        var total = subtotalAfterDiscount + totalIVA;

        // Validar totales
        if (total < 0)
        {
            throw new BusinessRuleException(AppMessages.Budget.InvalidTotal);
        }

        return new BudgetCalculation(items, subtotalAfterDiscount, totalIVA, total);
    }
}
