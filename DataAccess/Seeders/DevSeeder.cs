using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace DataAccess.Seeders;

public static class DevSeeder
{
    public static void SeedData(ProxarDbContext context)
    {
        // ============================================
        // RESET DE SECUENCIAS (SIEMPRE)
        // ============================================
        ResetSequences(context);

        // Verificar si ya hay datos
        if (context.Users.Any())
        {
            Console.WriteLine("⚠️  Base de datos ya tiene datos. Skipping seed.");
            return;
        }

        Console.WriteLine("🌱 Seeding data...");

        // ============================================
        // 0. COMPANY (Multi-tenant)
        // ============================================
        var company = new Company
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Aberturas Sagitario",
            Slug = "aberturas-sagitario",
            Active = true,
            CreatedAt = DateTime.UtcNow,
        };

        context.Companies.Add(company);
        context.SaveChanges(); // Guardar company primero
        Console.WriteLine("✅ Company creada");

        // ============================================
        // 1. USUARIOS
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

        var operador = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CompanyId = company.Id,
            Name = "Hermano",
            Email = "operador@proxar.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operador123!"),
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

        context.Users.AddRange(admin, operador, padre);
        Console.WriteLine("✅ 3 usuarios creados");

        // ============================================
        // 2. CLIENTES
        // ============================================
        var client1 = new Client
        {
            CompanyId = company.Id,
            Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"),
            Name = "Juan Pérez",
            Phone = "351-1234567",
            Email = "juan@example.com",
            Address = "Av. Libertador 1234, Córdoba",
            Notes = "Cliente habitual, paga siempre en efectivo",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            ModifiedAt = DateTime.UtcNow.AddDays(-30)
        };

        var client2 = new Client
        {
            CompanyId = company.Id,
            Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"),
            Name = "María González",
            Phone = "351-7654321",
            Email = "maria@example.com",
            Address = "Calle Falsa 123, Córdoba",
            Notes = "Prefiere transferencia",
            CreatedAt = DateTime.UtcNow.AddDays(-25),
            ModifiedAt = DateTime.UtcNow.AddDays(-25)
        };

        var client3 = new Client
        {
            CompanyId = company.Id,
            Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"),
            Name = "Local Comercial Centro",
            Phone = "351-9876543",
            Email = "local@centro.com",
            Address = "Av. Colón 450, Córdoba",
            Notes = "Dueño de 3 locales, cliente VIP",
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            ModifiedAt = DateTime.UtcNow.AddDays(-20)
        };

        var client4 = new Client
        {
            CompanyId = company.Id,
            Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"),
            Name = "Hotel Paradise",
            Phone = "351-5551234",
            Email = "info@hotelparadise.com",
            Address = "Av. Rafael Núñez 5000, Córdoba",
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            ModifiedAt = DateTime.UtcNow.AddDays(-15)
        };

        var client5 = new Client
        {
            CompanyId = company.Id,
            Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"),
            Name = "Ana Martínez",
            Phone = "351-4443322",
            Email = "ana.martinez@gmail.com",
            Address = "Barrio Cerro de las Rosas, Córdoba",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            ModifiedAt = DateTime.UtcNow.AddDays(-5)
        };

        context.Clients.AddRange(client1, client2, client3, client4, client5);
        Console.WriteLine("✅ 5 clientes creados");

        // ============================================
        // 3. CUENTAS
        // ============================================
        var cuentaEfectivo = new Account
        {
            CompanyId = company.Id,
            Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            Name = "Caja Efectivo",
            Type = AccountType.Efectivo,
            CurrentBalance = 125000,
            Active = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            ModifiedAt = DateTime.UtcNow
        };

        var cuentaBanco = new Account
        {
            CompanyId = company.Id,
            Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
            Name = "Banco Galicia",
            Type = AccountType.Banco,
            CurrentBalance = 450000,
            Active = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            ModifiedAt = DateTime.UtcNow
        };

        var cuentaMercadoPago = new Account
        {
            CompanyId = company.Id,
            Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
            Name = "MercadoPago",
            Type = AccountType.MercadoPago,
            CurrentBalance = 85000,
            Active = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            ModifiedAt = DateTime.UtcNow
        };

        context.Accounts.AddRange(cuentaEfectivo, cuentaBanco, cuentaMercadoPago);
        Console.WriteLine("✅ 3 cuentas creadas");

        // Guardar para tener IDs de Users, Clients y Accounts
        context.SaveChanges();

        // ============================================
        // 4. TICKETS
        // ============================================
        
        // Ticket 1: Completado
        var ticket1 = new Ticket
        {
            CompanyId = company.Id,
            Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"),
            Number = 1,
            ClientId = client1.Id,
            CreatedById = admin.Id,
            AssignedToId = operador.Id,
            Type = TicketType.Vidrio,
            Status = TicketState.Completado,
            Priority = Priority.Intermedia,
            Title = "Cambio de vidrio ventana cocina",
            Description = "Vidrio roto por piedra, 80x120cm aprox",
            Address = client1.Address,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            LastUpdatedAt = DateTime.UtcNow.AddDays(-8),
            CompletedAt = DateTime.UtcNow.AddDays(-8)
        };

        // Ticket 2: En Proceso
        var ticket2 = new Ticket
        {
            CompanyId = company.Id,
            Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
            Number = 2,
            ClientId = client3.Id,
            CreatedById = admin.Id,
            AssignedToId = padre.Id,
            Type = TicketType.Obra,
            Status = TicketState.EnProceso,
            Priority = Priority.Alta,
            Title = "Frente templado local comercial",
            Description = "Frente completo 8m x 3m, vidrio templado 10mm, 3 puertas",
            Address = client3.Address,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            LastUpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Ticket 3: Aprobado
        var ticket3 = new Ticket
        {
            CompanyId = company.Id,
            Id = Guid.Parse("b3333333-3333-3333-3333-333333333333"),
            Number = 3,
            ClientId = client4.Id,
            CreatedById = operador.Id,
            AssignedToId = admin.Id,
            Type = TicketType.Abertura,
            Status = TicketState.Aprobado,
            Priority = Priority.Alta,
            Title = "20 aberturas aluminio habitaciones hotel",
            Description = "Ventanas Modena corredizas 150x100, DVH",
            Address = client4.Address,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            LastUpdatedAt = DateTime.UtcNow.AddDays(-3)
        };

        // Ticket 4: Presupuestado
        var ticket4 = new Ticket
        {
            CompanyId = company.Id,
            Id = Guid.Parse("b4444444-4444-4444-4444-444444444444"),
            Number = 4,
            ClientId = client2.Id,
            CreatedById = padre.Id,
            AssignedToId = operador.Id,
            Type = TicketType.Reparacion,
            Status = TicketState.Presupuestado,
            Priority = Priority.Baja,
            Title = "Reparación mampara baño",
            Description = "Bisagra rota, necesita reemplazo",
            Address = client2.Address,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            LastUpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Ticket 5: Nuevo
        var ticket5 = new Ticket
        {
            CompanyId = company.Id,
            Id = Guid.Parse("b5555555-5555-5555-5555-555555555555"),
            Number = 5,
            ClientId = client5.Id,
            CreatedById = admin.Id,
            AssignedToId = admin.Id,
            Type = TicketType.Medicion,
            Status = TicketState.Nuevo,
            Priority = Priority.Intermedia,
            Title = "Medición para espejo de pared",
            Description = "Cliente quiere espejo completo en pared de living",
            Address = client5.Address,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastUpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Ticket 6: En Visita
        var ticket6 = new Ticket
        {
            CompanyId = company.Id,
            Id = Guid.Parse("b6666666-6666-6666-6666-666666666666"),
            Number = 6,
            ClientId = client1.Id,
            CreatedById = operador.Id,
            AssignedToId = padre.Id,
            Type = TicketType.Vidrio,
            Status = TicketState.EnVisita,
            Priority = Priority.Urgente,
            Title = "Vidrio roto por accidente",
            Description = "Urgente, necesita reemplazo hoy",
            Address = client1.Address,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        context.Tickets.AddRange(ticket1, ticket2, ticket3, ticket4, ticket5, ticket6);
        Console.WriteLine("✅ 6 tickets creados");

        // ============================================
        // 5. HISTORIAL DE TICKETS
        // ============================================
        var history1 = new TicketHistory
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            TicketId = ticket1.Id,
            UserId = admin.Id,
            Action = ActionHistorial.Creado,
            NewStatus = TicketState.Nuevo.ToString(),
            Timestamp = DateTime.UtcNow.AddDays(-10)
        };

        var history2 = new TicketHistory
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            TicketId = ticket1.Id,
            UserId = operador.Id,
            Action = ActionHistorial.EstadoCambiado,
            PreviousStatus = TicketState.Nuevo.ToString(),
            NewStatus = TicketState.EnProceso.ToString(),
            Comment = "Iniciando trabajo",
            Timestamp = DateTime.UtcNow.AddDays(-9)
        };

        var history3 = new TicketHistory
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            TicketId = ticket1.Id,
            UserId = operador.Id,
            Action = ActionHistorial.Completado,
            PreviousStatus = TicketState.EnProceso.ToString(),
            NewStatus = TicketState.Completado.ToString(),
            Comment = "Trabajo finalizado, cliente satisfecho",
            Timestamp = DateTime.UtcNow.AddDays(-8)
        };

        context.TicketHistory.AddRange(history1, history2, history3);
        Console.WriteLine("✅ Historial de tickets creado");

        // Guardar tickets e historial
        context.SaveChanges();

        // Resetear la secuencia de Number para que el próximo ticket use el valor correcto
        context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Tickets\"', 'Number'), (SELECT MAX(\"Number\") FROM \"Tickets\"))");

        // ============================================
        // 6. MOVIMIENTOS DE CAJA
        // ============================================

        // Movimiento 1: Ingreso por ticket completado
        var mov1 = new BoxMovement
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            Number = 1,
            AccountId = cuentaEfectivo.Id,
            TicketId = ticket1.Id,
            UserId = operador.Id,
            Type = MovementType.Ingreso,
            Amount = 15000,
            Method = PaymentMethod.Efectivo,
            Concept = "Cobro cambio vidrio - Ticket #1",
            VoucherNumber = "REC-001",
            MovementDate = DateTime.UtcNow.AddDays(-8),
            RegisteredAt = DateTime.UtcNow.AddDays(-8)
        };

        // Movimiento 2: Seña frente templado
        var mov2 = new BoxMovement
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            Number = 2,
            AccountId = cuentaBanco.Id,
            TicketId = ticket2.Id,
            UserId = padre.Id,
            Type = MovementType.Ingreso,
            Amount = 180000,
            Method = PaymentMethod.Transferencia,
            Concept = "Seña 60% frente templado - Ticket #2",
            VoucherNumber = "TRANS-5547",
            MovementDate = DateTime.UtcNow.AddDays(-6),
            RegisteredAt = DateTime.UtcNow.AddDays(-6)
        };

        // Movimiento 3: Seña aberturas hotel
        var mov3 = new BoxMovement
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            Number = 3,
            AccountId = cuentaBanco.Id,
            TicketId = ticket3.Id,
            UserId = admin.Id,
            Type = MovementType.Ingreso,
            Amount = 450000,
            Method = PaymentMethod.Transferencia,
            Concept = "Seña 60% aberturas hotel - Ticket #3",
            VoucherNumber = "TRANS-5601",
            Observations = "Cliente pagó adelantado 70% en vez de 60%",
            MovementDate = DateTime.UtcNow.AddDays(-3),
            RegisteredAt = DateTime.UtcNow.AddDays(-3)
        };

        // Movimiento 4: Compra de aluminio (egreso)
        var mov4 = new BoxMovement
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            Number = 4,
            AccountId = cuentaBanco.Id,
            TicketId = null,
            UserId = admin.Id,
            Type = MovementType.Egreso,
            Amount = 85000,
            Method = PaymentMethod.Transferencia,
            Concept = "Compra aluminio proveedor",
            VoucherNumber = "FAC-B-0012345",
            Observations = "Stock para proyecto hotel",
            MovementDate = DateTime.UtcNow.AddDays(-2),
            RegisteredAt = DateTime.UtcNow.AddDays(-2)
        };

        // Movimiento 5: Compra de vidrios (egreso)
        var mov5 = new BoxMovement
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            Number = 5,
            AccountId = cuentaEfectivo.Id,
            TicketId = null,
            UserId = padre.Id,
            Type = MovementType.Egreso,
            Amount = 28000,
            Method = PaymentMethod.Efectivo,
            Concept = "Compra vidrios float y templado",
            VoucherNumber = "FAC-B-7788",
            MovementDate = DateTime.UtcNow.AddDays(-1),
            RegisteredAt = DateTime.UtcNow.AddDays(-1)
        };

        // Movimiento 6: Pago de servicios (egreso)
        var mov6 = new BoxMovement
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            Number = 6,
            AccountId = cuentaMercadoPago.Id,
            TicketId = null,
            UserId = admin.Id,
            Type = MovementType.Egreso,
            Amount = 12500,
            Method = PaymentMethod.Transferencia,
            Concept = "Pago luz y gas taller",
            VoucherNumber = "SERV-2024-12",
            MovementDate = DateTime.UtcNow.AddHours(-5),
            RegisteredAt = DateTime.UtcNow.AddHours(-5)
        };

        // Movimiento 7: Ingreso efectivo hoy
        var mov7 = new BoxMovement
        {
            CompanyId = company.Id,
            Id = Guid.NewGuid(),
            Number = 7,
            AccountId = cuentaEfectivo.Id,
            TicketId = null,
            UserId = operador.Id,
            Type = MovementType.Ingreso,
            Amount = 8500,
            Method = PaymentMethod.Efectivo,
            Concept = "Venta espejo marco madera",
            VoucherNumber = "REC-002",
            Observations = "Cliente de paso, sin ticket previo",
            MovementDate = DateTime.UtcNow.AddHours(-2),
            RegisteredAt = DateTime.UtcNow.AddHours(-2)
        };

        context.BoxMovements.AddRange(mov1, mov2, mov3, mov4, mov5, mov6, mov7);
        Console.WriteLine("✅ 7 movimientos de caja creados");

        // ============================================
        // GUARDAR TODO
        // ============================================
        context.SaveChanges();

        Console.WriteLine("🎉 Seeding completado exitosamente!");
        Console.WriteLine($"   - Usuarios: 3");
        Console.WriteLine($"   - Clientes: 5");
        Console.WriteLine($"   - Cuentas: 3");
        Console.WriteLine($"   - Tickets: 6");
        Console.WriteLine($"   - Movimientos: 7");
        Console.WriteLine($"   - Saldo Total: ${cuentaEfectivo.CurrentBalance + cuentaBanco.CurrentBalance + cuentaMercadoPago.CurrentBalance:N0}");
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