using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        // Relación con Company (una empresa puede tener múltiples suscripciones históricas)
        builder.HasOne(s => s.Company)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Plan y estado
        builder.Property(s => s.Plan)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.MonthlyFee)
            .IsRequired()
            .HasPrecision(18, 2);

        // Mercado Pago
        builder.Property(s => s.MercadoPagoPreapprovalId)
            .HasMaxLength(100);

        builder.HasIndex(s => s.MercadoPagoPreapprovalId)
            .IsUnique()
            .HasFilter("\"MercadoPagoPreapprovalId\" IS NOT NULL");

        builder.Property(s => s.MercadoPagoCustomerId)
            .HasMaxLength(100);

        builder.Property(s => s.MercadoPagoCardToken)
            .HasMaxLength(255);

        builder.Property(s => s.LastFourDigits)
            .HasMaxLength(4);

        builder.Property(s => s.CardBrand)
            .HasMaxLength(50);

        builder.Property(s => s.CardHolderName)
            .HasMaxLength(200);

        // Metadata
        builder.Property(s => s.CancellationReason)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // Navegación
        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Subscription)
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
