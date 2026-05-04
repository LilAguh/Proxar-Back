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

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IPdfService _pdfService;
    private readonly IMapper _mapper;

    public BudgetService(
        IBudgetRepository budgetRepository,
        ITicketRepository ticketRepository,
        IClientRepository clientRepository,
        ICompanyRepository companyRepository,
        ICashRegisterRepository cashRegisterRepository,
        IPdfService pdfService,
        IMapper mapper)
    {
        _budgetRepository = budgetRepository;
        _ticketRepository = ticketRepository;
        _clientRepository = clientRepository;
        _companyRepository = companyRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _pdfService = pdfService;
        _mapper = mapper;
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

    public async Task<IEnumerable<BudgetDto>> GetByTicketIdAsync(Guid ticketId, Guid companyId)
    {
        var budgets = await _budgetRepository.GetByTicketIdAsync(ticketId, companyId);
        return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
    }

    public async Task<BudgetDto> CreateBudgetAsync(CreateBudgetRequest request, Guid userId, Guid companyId)
    {
        // Validar que el presupuesto tenga items
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new BusinessRuleException(AppMessages.Budget.NoItems);
        }

        // Validar ticket
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, companyId)
            ?? throw new NotFoundException(AppMessages.Ticket.NotFound);

        // Validar cliente
        var client = await _clientRepository.GetByIdAsync(ticket.ClientId, companyId)
            ?? throw new NotFoundException(AppMessages.Client.NotFound);

        // Obtener datos de la empresa para el PDF
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        // Obtener próximo número de presupuesto
        var nextNumber = await _budgetRepository.GetNextNumberAsync(companyId);

        // Calcular totales
        var items = new List<BudgetItem>();
        decimal subtotalBeforeDiscount = 0;
        decimal totalIVA = 0;

        foreach (var itemRequest in request.Items)
        {
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

        // Aplicar descuento al subtotal
        var subtotalAfterDiscount = subtotalBeforeDiscount - request.Discount;

        // Recalcular IVA si hay descuento
        if (request.Discount > 0)
        {
            // El descuento se aplica proporcionalmente a cada item
            var discountRatio = request.Discount / subtotalBeforeDiscount;
            totalIVA = 0;

            foreach (var item in items)
            {
                var itemDiscountedSubtotal = item.Subtotal * (1 - discountRatio);
                var itemIVA = itemDiscountedSubtotal * (item.IVAPercentage / 100);
                totalIVA += itemIVA;
            }
        }

        var total = subtotalAfterDiscount + totalIVA;

        // Validar totales
        if (total < 0)
        {
            throw new BusinessRuleException(AppMessages.Budget.InvalidTotal);
        }

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
            Subtotal = subtotalAfterDiscount,
            IVAAmount = totalIVA,
            Total = total,
            Discount = request.Discount,

            // Validez
            ValidDays = request.ValidDays,
            ValidUntil = DateTime.UtcNow.AddDays(request.ValidDays),

            // Estado inicial
            Status = BudgetStatus.Draft,

            // Items
            Items = items,

            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        // Guardar presupuesto en BD
        await _budgetRepository.AddAsync(budget);

        // Generar PDF
        try
        {
            var pdfPath = await _pdfService.GenerateBudgetPdfAsync(budget, company.Name);
            budget.PdfUrl = pdfPath;
            await _budgetRepository.UpdateAsync(budget);
        }
        catch (Exception ex)
        {
            throw new ApplicationException(AppMessages.Budget.PdfGenerationError, ex);
        }

        // Actualizar estado del ticket a Presupuestado
        if (ticket.Status != TicketState.Presupuestado)
        {
            ticket.Status = TicketState.Presupuestado;
            ticket.LastUpdatedAt = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket);
        }

        // Cambiar estado del presupuesto a Sent
        budget.Status = BudgetStatus.Sent;
        await _budgetRepository.UpdateAsync(budget);

        // Recargar con relaciones para mapear
        var createdBudget = await _budgetRepository.GetByIdAsync(budget.Id, companyId);
        return _mapper.Map<BudgetDto>(createdBudget!);
    }

    public async Task<BudgetDto> CreateDirectBudgetAsync(CreateDirectBudgetRequest request, Guid userId, Guid companyId)
    {
        // Validar que el presupuesto tenga items
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new BusinessRuleException(AppMessages.Budget.NoItems);
        }

        // Validar cliente
        var client = await _clientRepository.GetByIdAsync(request.ClientId, companyId)
            ?? throw new NotFoundException(AppMessages.Client.NotFound);

        // Obtener datos de la empresa para el PDF
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        // Verificar caja abierta (regla de negocio crítica)
        var businessDate = BusinessDateTime.GetBusinessDate(DateTime.UtcNow, company.TimeZoneId);
        var businessDateOnly = DateOnly.FromDateTime(businessDate);
        var cashRegister = await _cashRegisterRepository.GetTodayAsync(companyId, businessDateOnly);

        if (cashRegister == null || cashRegister.Status != CashRegisterStatus.Open)
        {
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

        await _ticketRepository.AddAsync(ticket);

        // Obtener próximo número de presupuesto
        var nextNumber = await _budgetRepository.GetNextNumberAsync(companyId);

        // Calcular totales
        var items = new List<BudgetItem>();
        decimal subtotalBeforeDiscount = 0;
        decimal totalIVA = 0;

        foreach (var itemRequest in request.Items)
        {
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

        // Aplicar descuento al subtotal
        var subtotalAfterDiscount = subtotalBeforeDiscount - request.Discount;

        // Recalcular IVA si hay descuento
        if (request.Discount > 0)
        {
            var discountRatio = request.Discount / subtotalBeforeDiscount;
            totalIVA = 0;

            foreach (var item in items)
            {
                var itemDiscountedSubtotal = item.Subtotal * (1 - discountRatio);
                var itemIVA = itemDiscountedSubtotal * (item.IVAPercentage / 100);
                totalIVA += itemIVA;
            }
        }

        var total = subtotalAfterDiscount + totalIVA;

        // Validar totales
        if (total < 0)
        {
            throw new BusinessRuleException(AppMessages.Budget.InvalidTotal);
        }

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
            Subtotal = subtotalAfterDiscount,
            IVAAmount = totalIVA,
            Total = total,
            Discount = request.Discount,

            // Validez
            ValidDays = request.ValidDays,
            ValidUntil = DateTime.UtcNow.AddDays(request.ValidDays),

            // Estado inicial
            Status = BudgetStatus.Sent,

            // Items
            Items = items,

            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        // Guardar presupuesto en BD
        await _budgetRepository.AddAsync(budget);

        // Generar PDF
        try
        {
            var pdfPath = await _pdfService.GenerateBudgetPdfAsync(budget, company.Name);
            budget.PdfUrl = pdfPath;
            await _budgetRepository.UpdateAsync(budget);
        }
        catch (Exception ex)
        {
            throw new ApplicationException(AppMessages.Budget.PdfGenerationError, ex);
        }

        // Recargar con relaciones para mapear
        var createdBudget = await _budgetRepository.GetByIdAsync(budget.Id, companyId);
        return _mapper.Map<BudgetDto>(createdBudget!);
    }

    public async Task<BudgetDto> UpdateStatusAsync(Guid id, BudgetStatus status, Guid companyId)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Budget.NotFound);

        await _budgetRepository.UpdateStatusAsync(id, status, companyId);

        var updatedBudget = await _budgetRepository.GetByIdAsync(id, companyId);
        return _mapper.Map<BudgetDto>(updatedBudget!);
    }

    public async Task<byte[]> GetBudgetPdfAsync(Guid id, Guid companyId)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, companyId)
            ?? throw new NotFoundException(AppMessages.Budget.NotFound);

        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        // Si existe el PDF guardado, leerlo
        if (!string.IsNullOrEmpty(budget.PdfUrl) && File.Exists(budget.PdfUrl))
        {
            return await File.ReadAllBytesAsync(budget.PdfUrl);
        }

        // Si no existe, regenerarlo
        return _pdfService.GenerateBudgetPdf(budget, company.Name);
    }
}
