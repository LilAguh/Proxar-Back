using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        // Identificación
        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LegalName)
            .HasMaxLength(200);

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Website)
            .HasMaxLength(500);

        builder.Property(c => c.Active)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Datos fiscales
        builder.Property(c => c.CUIT)
            .HasMaxLength(13); // 20-12345678-9

        builder.Property(c => c.IVA)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.IIBB)
            .HasMaxLength(20);

        builder.Property(c => c.FiscalAddress)
            .HasMaxLength(500);

        builder.Property(c => c.FiscalCity)
            .HasMaxLength(100);

        builder.Property(c => c.FiscalProvince)
            .HasMaxLength(100);

        builder.Property(c => c.FiscalPostalCode)
            .HasMaxLength(20);

        // Configuración AFIP
        builder.Property(c => c.CertPath)
            .HasMaxLength(500);

        builder.Property(c => c.CertPassword)
            .HasMaxLength(500);

        // Contacto
        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.MobilePhone)
            .HasMaxLength(50);

        builder.Property(c => c.SupportEmail)
            .HasMaxLength(255);

        // Configuración regional
        builder.Property(c => c.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.TimeZoneId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Language)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.DateFormat)
            .IsRequired()
            .HasMaxLength(20);

        // Suscripción
        builder.Property(c => c.SubscriptionStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.SubscriptionPlan)
            .HasMaxLength(50);

        builder.Property(c => c.MonthlyFee)
            .HasPrecision(18, 2);

        // Pasarela de pago
        builder.Property(c => c.PaymentGateway)
            .HasMaxLength(50);

        builder.Property(c => c.CustomerGatewayToken)
            .HasMaxLength(255);

        builder.Property(c => c.PaymentMethodToken)
            .HasMaxLength(255);

        builder.Property(c => c.LastFourDigits)
            .HasMaxLength(4);

        builder.Property(c => c.CardBrand)
            .HasMaxLength(50);

        // Soft delete
        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        // Navegación
        builder.HasMany(c => c.Users)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Clients)
            .WithOne(cl => cl.Company)
            .HasForeignKey(cl => cl.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Tickets)
            .WithOne(t => t.Company)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Accounts)
            .WithOne(a => a.Company)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.BoxMovements)
            .WithOne(bm => bm.Company)
            .HasForeignKey(bm => bm.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.CashRegisters)
            .WithOne(cr => cr.Company)
            .HasForeignKey(cr => cr.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
