using Models.Enums;

namespace Models;

public class SubscriptionPayment
{
    // Identificación
    public Guid Id { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Guid SubscriptionId { get; set; }
    public Guid CompanyId { get; set; }

    // Información del pago
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ARS";
    public PaymentStatus Status { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Mercado Pago
    public string? MercadoPagoPaymentId { get; set; } // ID del pago en MP
    public string? MercadoPagoStatus { get; set; } // approved, rejected, pending, etc.
    public string? MercadoPagoStatusDetail { get; set; } // cc_rejected_insufficient_amount, etc.

    // Respuesta de MP (webhook)
    public string? GatewayResponse { get; set; } // JSON completo de la respuesta
    public string? FailureReason { get; set; } // Razón legible del fallo

    // Metadata
    public DateTime AttemptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navegación
    public Subscription Subscription { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
