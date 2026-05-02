using Microsoft.EntityFrameworkCore;
using Models;
using DataAccess.Configurations;

namespace DataAccess.Context;

public class ProxarDbContext : DbContext
{
    public ProxarDbContext(DbContextOptions<ProxarDbContext> options) : base(options) { }

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
        // COMPANY
        // ============================================
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.Entity<Company>()
            .HasQueryFilter(e => e.Active && e.DeletedAt == null);

        // ============================================
        // SUBSCRIPTION
        // ============================================
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());

        // ============================================
        // SUBSCRIPTION PAYMENT
        // ============================================
        modelBuilder.ApplyConfiguration(new SubscriptionPaymentConfiguration());

        // ============================================
        // USER
        // ============================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.Email }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Users)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Soft delete global filter
            entity.HasQueryFilter(e => e.Active && e.DeletedAt == null);
        });

        // ============================================
        // CLIENT
        // ============================================
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.Active });
            entity.HasIndex(e => new { e.CompanyId, e.CreatedAt });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Clients)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Soft delete global filter
            entity.HasQueryFilter(e => e.Active && e.DeletedAt == null);
        });

        // ============================================
        // TICKET
        // ============================================
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique();
            entity.HasIndex(e => new { e.CompanyId, e.Status, e.Active });
            entity.HasIndex(e => new { e.CompanyId, e.CreatedAt });
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Address).HasMaxLength(300);

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Tickets)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Tickets)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.CreatedTickets)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedTo)
                  .WithMany(u => u.AssignedTickets)
                  .HasForeignKey(e => e.AssignedToId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Soft delete global filter
            entity.HasQueryFilter(e => e.Active && e.DeletedAt == null);
        });

        // ============================================
        // ACCOUNT
        // ============================================
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Accounts)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Soft delete global filter
            entity.HasQueryFilter(e => e.Active && e.DeletedAt == null);
        });

        // ============================================
        // BOX MOVEMENT
        // ============================================
        modelBuilder.Entity<BoxMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique();
            entity.HasIndex(e => new { e.CompanyId, e.Type, e.Active });
            entity.HasIndex(e => new { e.CompanyId, e.MovementDate });
            entity.HasIndex(e => new { e.CompanyId, e.RegisteredAt });
            
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Concept).IsRequired().HasMaxLength(300);
            entity.Property(e => e.VoucherNumber).HasMaxLength(50);
            entity.Property(e => e.Observations).HasMaxLength(1000);

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.BoxMovements)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Account)
                  .WithMany(a => a.Movements)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Ticket)
                  .WithMany(t => t.Movements)
                  .HasForeignKey(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.BoxMovements)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Soft delete global filter
            entity.HasQueryFilter(e => e.Active && e.DeletedAt == null);
        });

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
        // TICKET HISTORY
        // ============================================
        modelBuilder.Entity<TicketHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.PreviousStatus).HasMaxLength(50);
            entity.Property(e => e.NewStatus).HasMaxLength(50);

            entity.HasOne(e => e.Ticket)
                  .WithMany(t => t.History)
                  .HasForeignKey(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.TicketHistories)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
