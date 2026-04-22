using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(h => h.PreviousStatus)
            .HasMaxLength(50);

        builder.Property(h => h.NewStatus)
            .HasMaxLength(50);

        builder.Property(h => h.Timestamp)
            .IsRequired();

        // Relationships
        builder.HasOne(h => h.Ticket)
            .WithMany(t => t.History)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.User)
            .WithMany(u => u.ActionHistory)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(h => h.TicketId);
        builder.HasIndex(h => h.Timestamp);
    }
}
