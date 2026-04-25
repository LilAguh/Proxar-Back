using DataAccess.Context;
using Models;
using Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Seeders;

public static class ProductionSeeder
{
    public static void SeedData(ProxarDbContext context)
    {
        Console.WriteLine("🌱 Production Seeder - Starting...");

        // ============================================
        // RESET DE SECUENCIAS (SIEMPRE)
        // ============================================
        ResetSequences(context);

        // Verificar si ya hay datos
        if (context.Users.Any())
        {
            Console.WriteLine("⚠️  Base de datos ya tiene datos. Skipping production seed.");
            return;
        }

        Console.WriteLine("🏭 Seeding production data...");

        // ============================================
        // 1. USUARIOS REALES
        // ============================================
        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Agustín",
            Email = "admin@proxar.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var hermano = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Hermano",
            Email = "hermano@proxar.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Hermano123!"),
            Role = UserRole.Operador,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var padre = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Padre",
            Email = "padre@proxar.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Padre123!"),
            Role = UserRole.Operador,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.Users.AddRange(admin, hermano, padre);
        Console.WriteLine("✅ 3 usuarios creados");

        // ============================================
        // 2. CUENTAS REALES (CON SALDO 0)
        // ============================================
        var cuentaEfectivo = new Account
        {
            Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            Name = "Caja Efectivo",
            Type = AccountType.Efectivo,
            CurrentBalance = 0,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var cuentaBanco = new Account
        {
            Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
            Name = "Banco Galicia",
            Type = AccountType.Banco,
            CurrentBalance = 0,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var cuentaMercadoPago = new Account
        {
            Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
            Name = "MercadoPago",
            Type = AccountType.MercadoPago,
            CurrentBalance = 0,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.Accounts.AddRange(cuentaEfectivo, cuentaBanco, cuentaMercadoPago);
        Console.WriteLine("✅ 3 cuentas creadas (saldo $0)");

        context.SaveChanges();

        Console.WriteLine("🎉 Production seeding completado!");
        Console.WriteLine($"   - Usuarios: 3 (Admin, Hermano, Padre)");
        Console.WriteLine($"   - Cuentas: 3 (Efectivo, Banco, MercadoPago)");
        Console.WriteLine($"   - Clientes: 0 (se cargan manualmente)");
        Console.WriteLine($"   - Tickets: 0 (se crean en uso real)");
        Console.WriteLine($"   - Movimientos: 0 (se registran después)");
    }

    private static void ResetSequences(ProxarDbContext context)
    {
        try
        {
            // Reset Tickets.Number sequence
            context.Database.ExecuteSqlRaw(@"
                SELECT setval(
                    pg_get_serial_sequence('""Tickets""', 'Number'), 
                    COALESCE((SELECT MAX(""Number"") FROM ""Tickets""), 0) + 1, 
                    false
                );
            ");

            // Reset BoxMovements.Number sequence
            context.Database.ExecuteSqlRaw(@"
                SELECT setval(
                    pg_get_serial_sequence('""BoxMovements""', 'Number'), 
                    COALESCE((SELECT MAX(""Number"") FROM ""BoxMovements""), 0) + 1, 
                    false
                );
            ");

            Console.WriteLine("✅ Secuencias reseteadas correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error reseteando secuencias: {ex.Message}");
        }
    }
}