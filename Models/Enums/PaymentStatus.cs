namespace Models.Enums;

public enum PaymentStatus
{
    Pending = 1,
    Success = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5,
    Chargeback = 6
}
