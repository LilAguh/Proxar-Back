using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Context;

public class ProxarDbContext : DbContext
{
    public ProxarDbContext(DbContextOptions<ProxarDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketHistory> TicketHistory { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<BoxMovement> BoxMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProxarDbContext).Assembly);
    }
}