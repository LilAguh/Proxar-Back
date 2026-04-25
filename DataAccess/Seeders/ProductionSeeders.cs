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

        ResetSequences(context);

        if (context.Companies.Any())
        {
            Console.WriteLine("⚠️  Ya existen empresas. Skipping seed.");
            return;
        }

        Console.WriteLine("🏭 Seeding production data...");

        // ============================================
        // EMPRESA
        // ============================================
        var company = new Company
        {
            Id = Guid.Parse("c0000000-0000-0000-0000-000000000001"),
            Name = "Aberturas Sagitario",
            Slug = "aberturas-sagitario",
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Companies.Add(company);
        Console.WriteLine("✅ Empresa creada: Aberturas Sagitario");

        // ============================================
        // USUARIOS
        // ============================================
        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CompanyId = company.Id,
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
            CompanyId = company.Id,
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
            CompanyId = company.Id,
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
        // CUENTAS
        // ============================================
        var cuentaEfectivo = new Account
        {
            Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            CompanyId = company.Id,
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
            CompanyId = company.Id,
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
            CompanyId = company.Id,
            Name = "MercadoPago",
            Type = AccountType.MercadoPago,
            CurrentBalance = 0,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.Accounts.AddRange(cuentaEfectivo, cuentaBanco, cuentaMercadoPago);
        Console.WriteLine("✅ 3 cuentas creadas");

        context.SaveChanges();

        Console.WriteLine("🎉 Production seeding completado!");
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