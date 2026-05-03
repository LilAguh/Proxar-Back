using Microsoft.EntityFrameworkCore;
using Models;
using DataAccess.Configurations;
using DataAccess.Converters;
using DataAccess.Services;

namespace DataAccess.Context;

public class ProxarDbContext : DbContext
{
    private readonly IEncryptionService _encryptionService;

    public ProxarDbContext(
        DbContextOptions<ProxarDbContext> options,
        IEncryptionService encryptionService) : base(options)
    {
        _encryptionService = encryptionService;
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketHistory> TicketHistory { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<BoxMovement> BoxMovements { get; set; }
    public DbSet<CashRegister> CashRegisters { get; set; }
    public DbSet<CashRegisterEntry> CashRegisterEntries { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // APPLY CONFIGURATIONS (enums → string)
        // ============================================
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new BoxMovementConfiguration());
        modelBuilder.ApplyConfiguration(new TicketHistoryConfiguration());

        // ============================================
        // ENCRYPTION (Value Converters for sensitive fields)
        // ============================================
        var encryptedConverter = new EncryptedStringConverter(_encryptionService);

        // Company: CertPassword (AFIP certificate password)
        modelBuilder.Entity<Company>()
            .Property(e => e.CertPassword)
            .HasConversion(encryptedConverter);

        // Subscription: Mercado Pago sensitive tokens
        modelBuilder.Entity<Subscription>()
            .Property(e => e.MercadoPagoCardToken)
            .HasConversion(encryptedConverter);

        modelBuilder.Entity<Subscription>()
            .Property(e => e.MercadoPagoPreapprovalId)
            .HasConversion(encryptedConverter);

        modelBuilder.Entity<Subscription>()
            .Property(e => e.MercadoPagoCustomerId)
            .HasConversion(encryptedConverter);

        // ============================================
        // SOFT DELETE QUERY FILTERS
        // ============================================
        modelBuilder.Entity<Company>()
            .HasQueryFilter(e => e.Active && e.DeletedAt == null);

        modelBuilder.Entity<User>()
            .HasQueryFilter(e => e.Active && e.DeletedAt == null);

        modelBuilder.Entity<Client>()
            .HasQueryFilter(e => e.Active && e.DeletedAt == null);

        modelBuilder.Entity<Ticket>()
            .HasQueryFilter(e => e.Active && e.DeletedAt == null);

        modelBuilder.Entity<Account>()
            .HasQueryFilter(e => e.Active && e.DeletedAt == null);

        modelBuilder.Entity<BoxMovement>()
            .HasQueryFilter(e => e.Active && e.DeletedAt == null);

        // ============================================
        // ADDITIONAL CONFIGURATIONS
        // ============================================

        // User: índice único por empresa y email
        modelBuilder.Entity<User>()
            .HasIndex(e => new { e.CompanyId, e.Email })
            .IsUnique();

        // User: relación con Company
        modelBuilder.Entity<User>()
            .HasOne(e => e.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Client: índices adicionales por empresa
        modelBuilder.Entity<Client>()
            .HasIndex(e => new { e.CompanyId, e.Active });

        modelBuilder.Entity<Client>()
            .HasIndex(e => new { e.CompanyId, e.CreatedAt });

        // Client: relación con Company
        modelBuilder.Entity<Client>()
            .HasOne(e => e.Company)
            .WithMany(c => c.Clients)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket: índice único por empresa y número
        modelBuilder.Entity<Ticket>()
            .HasIndex(e => new { e.CompanyId, e.Number })
            .IsUnique();

        // Ticket: relación con Company
        modelBuilder.Entity<Ticket>()
            .HasOne(e => e.Company)
            .WithMany(c => c.Tickets)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Account: relación con Company
        modelBuilder.Entity<Account>()
            .HasOne(e => e.Company)
            .WithMany(c => c.Accounts)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // BoxMovement: índice único por empresa y número
        modelBuilder.Entity<BoxMovement>()
            .HasIndex(e => new { e.CompanyId, e.Number })
            .IsUnique();

        // BoxMovement: relación con Company
        modelBuilder.Entity<BoxMovement>()
            .HasOne(e => e.Company)
            .WithMany(c => c.BoxMovements)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ============================================
        // CASH REGISTER
        // ============================================
        modelBuilder.Entity<CashRegister>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(10);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => new { e.CompanyId, e.Date }).IsUnique();

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.CashRegisters)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.OpenedBy)
                  .WithMany()
                  .HasForeignKey(e => e.OpenedById)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ClosedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ClosedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CashRegisterEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OpeningAmount).HasPrecision(18, 2);
            entity.Property(e => e.ClosingAmount).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.CashRegisterId, e.AccountId }).IsUnique();

            entity.HasOne(e => e.CashRegister)
                  .WithMany(r => r.Entries)
                  .HasForeignKey(e => e.CashRegisterId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                  .WithMany(a => a.CashRegisterEntries)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================
        // REFRESH TOKEN
        // ============================================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            entity.Ignore(e => e.IsRevoked);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsActive);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // CONCURRENCIA OPTIMISTA (RowVersion)
        // ============================================
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetProperty("RowVersion") is null)
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .Property<byte[]>("RowVersion")
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
