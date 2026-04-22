using Models.Enums;

namespace Services.DTOs.Requests;

public class RegisterMovementRequest
{
    public Guid AccountId { get; set; }
    public Guid? TicketId { get; set; }
    public MovementType Type { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string Concept { get; set; } = string.Empty;
    public string? VoucherNumber { get; set; }
    public string? Observations { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}