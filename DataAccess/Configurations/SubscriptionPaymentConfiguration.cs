using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.HasKey(sp => sp.Id);

        // Relaciones
        builder.HasOne(sp => sp.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(sp => sp.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.Company)
            .WithMany(c => c.SubscriptionPayments)
            .HasForeignKey(sp => sp.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Información del pago
        builder.Property(sp => sp.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sp => sp.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(sp => sp.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Mercado Pago
        builder.Property(sp => sp.MercadoPagoPaymentId)
            .HasMaxLength(100);

        builder.HasIndex(sp => sp.MercadoPagoPaymentId)
            .IsUnique()
            .HasFilter("\"MercadoPagoPaymentId\" IS NOT NULL");

        builder.Property(sp => sp.MercadoPagoStatus)
            .HasMaxLength(50);

        builder.Property(sp => sp.MercadoPagoStatusDetail)
            .HasMaxLength(100);

        builder.Property(sp => sp.GatewayResponse)
            .HasColumnType("TEXT"); // JSON completo

        builder.Property(sp => sp.FailureReason)
            .HasMaxLength(500);

        // Metadata
        builder.Property(sp => sp.AttemptedAt)
            .IsRequired();

        builder.Property(sp => sp.CreatedAt)
            .IsRequired();

        // Índices para búsquedas comunes
        builder.HasIndex(sp => new { sp.CompanyId, sp.Status, sp.CreatedAt });
    }
}
