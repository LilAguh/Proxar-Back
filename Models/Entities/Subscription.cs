using Models.Enums;

namespace Models;

public class Subscription
{
    // Identificación
    public Guid Id { get; set; }
    public byte[]? RowVersion { get; set; } = [];
    public Guid CompanyId { get; set; }

    // Plan y estado
    public SubscriptionPlan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal MonthlyFee { get; set; } // Precio en ARS

    // Trial
    public bool IsOnTrial { get; set; }
    public DateTime? TrialStartedAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }

    // Ciclo de facturación
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CancellationEffectiveDate { get; set; } // Cuando termina el período pagado

    // Mercado Pago
    public string? MercadoPagoPreapprovalId { get; set; } // ID de suscripción en MP
    public string? MercadoPagoCustomerId { get; set; } // ID del customer en MP
    public string? MercadoPagoCardToken { get; set; } // Token de la tarjeta
    public string? LastFourDigits { get; set; }
    public string? CardBrand { get; set; } // visa, mastercard, etc.
    public string? CardHolderName { get; set; }

    // Intentos de cobro
    public int FailedPaymentAttempts { get; set; } // Contador de fallos consecutivos
    public DateTime? LastPaymentAttemptAt { get; set; }
    public DateTime? LastSuccessfulPaymentAt { get; set; }

    // Metadata
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navegación
    public Company Company { get; set; } = null!;
    public ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
}
