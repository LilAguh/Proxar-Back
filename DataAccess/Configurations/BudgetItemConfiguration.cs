using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
    public void Configure(EntityTypeBuilder<BudgetItem> builder)
    {
        builder.HasKey(bi => bi.Id);

        builder.Property(bi => bi.Quantity)
            .IsRequired();

        builder.Property(bi => bi.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(bi => bi.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bi => bi.IVAPercentage)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(21);

        builder.Property(bi => bi.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bi => bi.IVAAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bi => bi.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        // Indexes
        builder.HasIndex(bi => bi.BudgetId);

        // Relationships
        builder.HasOne(bi => bi.Budget)
            .WithMany(b => b.Items)
            .HasForeignKey(bi => bi.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
