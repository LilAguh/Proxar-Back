using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Number)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.HasIndex(b => new { b.CompanyId, b.Number })
            .IsUnique();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.ClientPhone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.ClientCUIT)
            .HasMaxLength(20);

        builder.Property(b => b.ClientEmail)
            .HasMaxLength(200);

        builder.Property(b => b.ClientAddress)
            .HasMaxLength(500);

        builder.Property(b => b.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.IVAAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Discount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(b => b.ValidDays)
            .HasDefaultValue(15);

        builder.Property(b => b.ValidUntil)
            .IsRequired();

        builder.Property(b => b.PdfUrl)
            .HasMaxLength(500);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.ModifiedAt)
            .IsRequired();

        // Concurrency token
        builder.Property(b => b.RowVersion)
            .IsRowVersion()
            .HasDefaultValueSql("'\\x00000000000000000000000000000001'::bytea");

        // Indexes
        builder.HasIndex(b => b.CompanyId);
        builder.HasIndex(b => b.TicketId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CreatedAt);
        builder.HasIndex(b => new { b.CompanyId, b.Status, b.Active });

        // Relationships
        builder.HasOne(b => b.Company)
            .WithMany()
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Ticket)
            .WithMany()
            .HasForeignKey(b => b.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Client)
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.CreatedBy)
            .WithMany()
            .HasForeignKey(b => b.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Budget)
            .HasForeignKey(i => i.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter para soft delete
        builder.HasQueryFilter(b => b.Active);
    }
}
