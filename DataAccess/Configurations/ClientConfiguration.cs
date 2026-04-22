using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace DataAccess.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.ModifiedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(c => c.Tickets)
            .WithOne(t => t.Client)
            .HasForeignKey(t => t.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}