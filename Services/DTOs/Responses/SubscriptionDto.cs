namespace Services.DTOs.Responses;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    // Plan y estado
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal MonthlyFee { get; set; }

    // Trial
    public bool IsOnTrial { get; set; }
    public DateTime? TrialStartedAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }

    // Ciclo de facturación
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CancellationEffectiveDate { get; set; }

    // Tarjeta (datos públicos)
    public string? LastFourDigits { get; set; }
    public string? CardBrand { get; set; }
    public string? CardHolderName { get; set; }

    // Intentos de cobro
    public int FailedPaymentAttempts { get; set; }
    public DateTime? LastPaymentAttemptAt { get; set; }
    public DateTime? LastSuccessfulPaymentAt { get; set; }

    // Metadata
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
