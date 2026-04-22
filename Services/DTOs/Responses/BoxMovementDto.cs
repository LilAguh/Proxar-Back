using Models.Enums;

namespace Services.DTOs.Responses;

public class BoxMovementDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public AccountDto Account { get; set; } = null!;
    public int? TicketNumber { get; set; }
    public UserDto User { get; set; } = null!;
    public MovementType Type { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string Concept { get; set; } = string.Empty;
    public string? VoucherNumber { get; set; }
    public string? Observations { get; set; }
    public DateTime MovementDate { get; set; }
    public DateTime RegisteredAt { get; set; }
}