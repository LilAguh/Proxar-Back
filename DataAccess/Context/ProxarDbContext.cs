using Microsoft.EntityFrameworkCore;
using Models;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // COMPANY
        // ============================================
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            
            // Soft delete global filter
            entity.HasQueryFilter(e => e.Active && e.DeletedAt == null);
        });

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