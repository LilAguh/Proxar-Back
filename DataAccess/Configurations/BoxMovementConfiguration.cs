using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class BoxMovementConfiguration : IEntityTypeConfiguration<BoxMovement>
{
    public void Configure(EntityTypeBuilder<BoxMovement> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Number)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.HasIndex(m => m.Number)
            .IsUnique();

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(m => m.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(m => m.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Concept)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.VoucherNumber)
            .HasMaxLength(100);

        builder.Property(m => m.MovementDate)
            .IsRequired();

        builder.Property(m => m.RegisteredAt)
            .IsRequired();

        // Check constraint: Amount > 0
        builder.ToTable(t => t.HasCheckConstraint("CK_BoxMovement_Amount", "\"Amount\" > 0"));

        // Relationships
        builder.HasOne(m => m.Account)
            .WithMany(a => a.Movements)
            .HasForeignKey(m => m.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Ticket)
            .WithMany(t => t.Movements)
            .HasForeignKey(m => m.TicketId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.User)
            .WithMany(u => u.BoxMovements)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => m.AccountId);
        builder.HasIndex(m => m.TicketId);
        builder.HasIndex(m => m.MovementDate);
    }
}
