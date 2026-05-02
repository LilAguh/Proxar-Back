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

        // ============================================
        // SUSCRIPCIONES (SIEMPRE) - para empresas existentes
        // ============================================
        EnsureSubscriptions(context);

        // Verificar si ya hay datos
        if (context.Users.Any())
        {
            Console.WriteLine("⚠️  Base de datos ya tiene datos. Skipping seed.");
            return;
        }

        Console.WriteLine("🌱 Seeding data...");

        // ============================================
        // 0. COMPANIES (Multi-tenant - 3 empresas)
        // ============================================
        var company1 = new Company
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Aberturas Sagitario",
            LegalName = "Aberturas Sagitario S.R.L.",
            Slug = "sagitario",
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            // Datos fiscales
            CUIT = "30-71234567-8",
            IVA = IVACondition.ResponsableInscripto,
            IIBB = "CM-123456",
            FiscalAddress = "Av. Colón 1234",
            FiscalCity = "Córdoba",
            FiscalProvince = "Córdoba",
            FiscalPostalCode = "X5000",
            StartOfActivities = new DateTime(1989, 3, 15),
            DefaultSalesPoint = 1,

            // Contacto
            Email = "contacto@sagitario.com.ar",
            Phone = "+54 351 4567890",
            MobilePhone = "+54 9 351 6789012",
            SupportEmail = "soporte@sagitario.com.ar",

            // Configuración regional
            Currency = "ARS",
            TimeZoneId = "America/Argentina/Buenos_Aires",
            Language = "es-AR",
            DateFormat = "dd/MM/yyyy",
        };

        var company2 = new Company
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "Vidrios del Norte",
            LegalName = "Vidrios del Norte S.A.",
            Slug = "vidrios-norte",
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            // Datos fiscales
            CUIT = "30-65432109-4",
            IVA = IVACondition.ResponsableInscripto,
            IIBB = "SA-987654",
            FiscalAddress = "Ruta 9 Km 1240",
            FiscalCity = "Salta",
            FiscalProvince = "Salta",
            FiscalPostalCode = "A4400",
            StartOfActivities = new DateTime(2005, 7, 20),
            DefaultSalesPoint = 1,

            // Contacto
            Email = "info@vidriosdelnorte.com",
            Phone = "+54 387 4321000",

            // Configuración regional
            Currency = "ARS",
            TimeZoneId = "America/Argentina/Buenos_Aires",
            Language = "es-AR",
            DateFormat = "dd/MM/yyyy",
        };

        var company3 = new Company
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "AlumCor S.A.",
            LegalName = "AlumCor Sociedad Anónima",
            Slug = "alumcor",
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            // Datos fiscales
            CUIT = "30-55555555-5",
            IVA = IVACondition.Monotributista,
            FiscalAddress = "Bv. San Juan 567",
            FiscalCity = "Córdoba",
            FiscalProvince = "Córdoba",
            FiscalPostalCode = "X5000",
            StartOfActivities = new DateTime(2018, 11, 1),

            // Contacto
            Email = "ventas@alumcor.com.ar",
            Phone = "+54 351 4111222",

            // Configuración regional
            Currency = "ARS",
            TimeZoneId = "America/Argentina/Buenos_Aires",
            Language = "es-AR",
            DateFormat = "dd/MM/yyyy",
        };

        context.Companies.AddRange(company1, company2, company3);
        context.SaveChanges(); // Guardar companies primero
        Console.WriteLine("✅ 3 empresas creadas");

        // ============================================
        // SUSCRIPCIONES
        // ============================================
        var subscription1 = new Subscription
        {
            Id = Guid.NewGuid(),
            CompanyId = company1.Id,
            Plan = SubscriptionPlan.Pro,
            Status = SubscriptionStatus.Trial,
            MonthlyFee = 49999m,
            IsOnTrial = true,
            TrialStartedAt = DateTime.UtcNow.AddDays(-1),
            TrialEndsAt = DateTime.UtcNow.AddDays(29),
            CurrentPeriodStart = DateTime.UtcNow.AddDays(-1),
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(29),
            NextBillingDate = DateTime.UtcNow.AddDays(30),
            FailedPaymentAttempts = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        var subscription2 = new Subscription
        {
            Id = Guid.NewGuid(),
            CompanyId = company2.Id,
            Plan = SubscriptionPlan.Basic,
            Status = SubscriptionStatus.Active,
            MonthlyFee = 29999m,
            IsOnTrial = false,
            CurrentPeriodStart = DateTime.UtcNow.AddDays(-15),
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
            NextBillingDate = DateTime.UtcNow.AddDays(16),
            MercadoPagoPreapprovalId = "fake-preapproval-id-123",
            MercadoPagoCustomerId = "fake-customer-id-456",
            LastFourDigits = "1234",
            CardBrand = "visa",
            CardHolderName = "Carlos Vidrios",
            FailedPaymentAttempts = 0,
            LastSuccessfulPaymentAt = DateTime.UtcNow.AddMonths(-1),
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            UpdatedAt = DateTime.UtcNow
        };

        var subscription3 = new Subscription
        {
            Id = Guid.NewGuid(),
            CompanyId = company3.Id,
            Plan = SubscriptionPlan.Basic,
            Status = SubscriptionStatus.Trial,
            MonthlyFee = 29999m,
            IsOnTrial = true,
            TrialStartedAt = DateTime.UtcNow.AddDays(-16),
            TrialEndsAt = DateTime.UtcNow.AddDays(-1), // Trial ya expiró (debería pasar a Expired)
            CurrentPeriodStart = DateTime.UtcNow.AddDays(-16),
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(-1),
            FailedPaymentAttempts = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-16),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.Subscriptions.AddRange(subscription1, subscription2, subscription3);
        context.SaveChanges();
        Console.WriteLine("✅ 3 suscripciones creadas");

        // Crear un pago de ejemplo para Vidrios del Norte
        var payment1 = new SubscriptionPayment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription2.Id,
            CompanyId = company2.Id,
            Amount = 29999m,
            Currency = "ARS",
            Status = PaymentStatus.Success,
            PeriodStart = DateTime.UtcNow.AddMonths(-2).AddDays(-15),
            PeriodEnd = DateTime.UtcNow.AddMonths(-1).AddDays(-15),
            MercadoPagoPaymentId = "fake-payment-id-789",
            MercadoPagoStatus = "approved",
            CompletedAt = DateTime.UtcNow.AddMonths(-1),
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        context.SubscriptionPayments.Add(payment1);
        context.SaveChanges();
        Console.WriteLine("✅ 1 pago de suscripción creado");

        // Usar company1 (Sagitario) como empresa principal para los datos de prueba
        var company = company1;

        // ============================================
        // 1. USUARIOS (5 empleados)
        // ============================================
        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CompanyId = company.Id,
            Name = "Admin",
            Email = "admin@sagitario.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234"),
            Role = UserRole.Admin,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var operador = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CompanyId = company.Id,
            Name = "Daniel",
            Email = "operador@sagitario.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operador1234"),
            Role = UserRole.Operador,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var visor = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CompanyId = company.Id,
            Name = "Luca",
            Email = "visor@sagitario.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Visor1234"),
            Role = UserRole.Visor,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var operador2 = new User
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            CompanyId = company.Id,
            Name = "Martina",
            Email = "martina@sagitario.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Martina1234"),
            Role = UserRole.Operador,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var operador3 = new User
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            CompanyId = company.Id,
            Name = "Roberto",
            Email = "roberto@sagitario.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Roberto1234"),
            Role = UserRole.Operador,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        // Usuarios de las otras empresas (solo admins para acceso rápido)
        var admin2 = new User
        {
            Id = Guid.NewGuid(),
            CompanyId = company2.Id,
            Name = "Carlos Vidrios",
            Email = "admin@vidriosnorte.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234"),
            Role = UserRole.Admin,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var admin3 = new User
        {
            Id = Guid.NewGuid(),
            CompanyId = company3.Id,
            Name = "Patricia Aluminio",
            Email = "admin@alumcor.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234"),
            Role = UserRole.Admin,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.Users.AddRange(admin, operador, visor, operador2, operador3, admin2, admin3);
        Console.WriteLine("✅ 7 usuarios creados (5 en Sagitario, 1 en Vidrios Norte, 1 en AlumCor)");

        // ============================================
        // 2. CLIENTES (50 clientes)
        // ============================================
        var clients = new List<Client>();
        var clientNames = new[] {
            "Juan Pérez", "María González", "Carlos Rodríguez", "Ana Martínez", "Luis López",
            "Laura Fernández", "Diego Silva", "Carolina Díaz", "Martín Castro", "Valentina Romero",
            "Gabriel Sosa", "Sofía Benítez", "Matías Giménez", "Camila Morales", "Lucas Ruiz",
            "Florencia Medina", "Nicolás Vargas", "Lucía Peralta", "Facundo Arias", "Agustina Torres",
            "Hotel Paradise", "Restaurant El Buen Sabor", "Supermercado La Familia", "Farmacia Central",
            "Veterinaria Pet Care", "Estudio Jurídico López & Asoc", "Consultorio Médico Salud Plus",
            "Gimnasio Fitness Center", "Peluquería Estilo Nuevo", "Taller Mecánico El Rápido",
            "Panadería Don Juan", "Librería Mundo Libro", "Ferretería Todo Construcción", "Pizzería La Nonna",
            "Bar El Encuentro", "Café & Té La Taza", "Boutique Moda Actual", "Zapatería Pie Cómodo",
            "Electrónica Tech World", "Mueblería El Hogar", "Óptica Visión Clara", "Joyería Brillantes",
            "Floristería Jardín Secreto", "Carnicería La Vaca Feliz", "Verdulería Fresco Verde",
            "Pescadería Mar Azul", "Heladería Cremoso", "Rotisería Casera", "Lavadero Auto Limpio",
            "Cerrajería 24hs"
        };

        var random = new Random(42); // Seed fijo para reproducibilidad
        for (int i = 0; i < 50; i++)
        {
            var name = clientNames[i];
            var isCompany = name.Contains("Hotel") || name.Contains("Restaurant") ||
                           name.Contains("Supermercado") || name.Contains("Farmacia") ||
                           name.Contains("Veterinaria") || name.Contains("Estudio") ||
                           name.Contains("Consultorio") || name.Contains("Gimnasio") ||
                           name.Contains("Peluquería") || name.Contains("Taller") ||
                           name.Contains("Panadería") || name.Contains("Librería") ||
                           name.Contains("Ferretería") || name.Contains("Pizzería") ||
                           name.Contains("Bar") || name.Contains("Café") || name.Contains("Boutique") ||
                           name.Contains("Zapatería") || name.Contains("Electrónica") ||
                           name.Contains("Mueblería") || name.Contains("Óptica") ||
                           name.Contains("Joyería") || name.Contains("Floristería") ||
                           name.Contains("Carnicería") || name.Contains("Verdulería") ||
                           name.Contains("Pescadería") || name.Contains("Heladería") ||
                           name.Contains("Rotisería") || name.Contains("Lavadero") ||
                           name.Contains("Cerrajería");

            clients.Add(new Client
            {
                CompanyId = company.Id,
                Id = Guid.NewGuid(),
                Name = name,
                Phone = $"351-{random.Next(1000000, 9999999)}",
                Email = isCompany ? $"info@{name.ToLower().Replace(" ", "").Replace("&", "")}.com" :
                                   $"{name.Split(' ')[0].ToLower()}@gmail.com",
                Address = $"{(isCompany ? "Av." : "Calle")} {random.Next(100, 9999)}, Córdoba",
                Notes = isCompany ? "Cliente comercial" : (random.Next(0, 2) == 0 ? "Cliente habitual" : null),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                ModifiedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
            });
        }

        context.Clients.AddRange(clients);
        Console.WriteLine("✅ 50 clientes creados");

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
        // 4. TICKETS (40 tickets)
        // ============================================
        var tickets = new List<Ticket>();
        var ticketTitles = new[] {
            "Cambio de vidrio ventana cocina", "Frente templado local comercial", "Aberturas aluminio habitaciones",
            "Reparación mampara baño", "Medición para espejo de pared", "Vidrio roto por accidente",
            "Instalación puerta balcón", "Cambio de cerradura ventana", "Espejo de cuerpo entero",
            "Ventana corrediza living", "Mampara shower box", "Pasamanos vidrio escalera",
            "Frente vidriado negocio", "Ventanas DVH dormitorios", "Reparación ventiluz",
            "Vidrio mesa comedor", "Pérgola techo policarbonato", "Cerramiento balcón",
            "Espejo baño con repisa", "Vitrina exhibición", "Puerta vidrio templado oficina",
            "Reemplazo vidrio puerta entrada", "Ventanas oscilobatientes cocina", "Baranda vidrio terraza",
            "Espejo decorativo recibidor", "Mampara bañera fija", "Vidrio estufa hogar",
            "Frente comercial aluminio", "Ventanas guillotina antiguas", "Pérgola retráctil patio",
            "Espejo biselado living", "Cortina vidrio local", "Techo vidrio galería",
            "Reparación ventana techo", "Biombo divisor ambiente", "Mosquitero ventana",
            "Puerta vaivén cocina", "Vidriera escaparate", "Tragaluz claraboya", "Mamparas sanitarias"
        };

        var users = new[] { admin, operador, visor, operador2, operador3 };
        var ticketTypes = Enum.GetValues<TicketType>();
        var ticketStates = Enum.GetValues<TicketState>();
        var priorities = Enum.GetValues<Priority>();

        for (int i = 0; i < 40; i++)
        {
            var daysAgo = random.Next(0, 90);
            var state = ticketStates[random.Next(ticketStates.Length)];
            var createdAt = DateTime.UtcNow.AddDays(-daysAgo);

            tickets.Add(new Ticket
            {
                CompanyId = company.Id,
                Id = Guid.NewGuid(),
                Number = i + 1,
                ClientId = clients[random.Next(clients.Count)].Id,
                CreatedById = users[random.Next(users.Length)].Id,
                AssignedToId = users[random.Next(users.Length)].Id,
                Type = ticketTypes[random.Next(ticketTypes.Length)],
                Status = state,
                Priority = priorities[random.Next(priorities.Length)],
                Title = ticketTitles[i],
                Description = $"Descripción detallada del trabajo: {ticketTitles[i]}",
                Address = clients[random.Next(clients.Count)].Address,
                CreatedAt = createdAt,
                LastUpdatedAt = createdAt.AddDays(random.Next(0, (int)(DateTime.UtcNow - createdAt).TotalDays + 1)),
                CompletedAt = state == TicketState.Completado ? createdAt.AddDays(random.Next(1, 15)) : null
            });
        }

        context.Tickets.AddRange(tickets);
        Console.WriteLine("✅ 40 tickets creados");

        // ============================================
        // 5. HISTORIAL DE TICKETS
        // ============================================
        var ticketHistories = new List<TicketHistory>();
        var historyActions = Enum.GetValues<ActionHistorial>();

        // Crear historial para los primeros 15 tickets (algunos con historial, otros sin)
        for (int i = 0; i < Math.Min(15, tickets.Count); i++)
        {
            var ticket = tickets[i];
            var historyCount = random.Next(1, 5); // 1 a 4 entradas de historial por ticket

            for (int h = 0; h < historyCount; h++)
            {
                ticketHistories.Add(new TicketHistory
                {
                    CompanyId = company.Id,
                    Id = Guid.NewGuid(),
                    TicketId = ticket.Id,
                    UserId = users[random.Next(users.Length)].Id,
                    Action = historyActions[random.Next(historyActions.Length)],
                    PreviousStatus = h > 0 ? ticketStates[random.Next(ticketStates.Length)].ToString() : null,
                    NewStatus = ticketStates[random.Next(ticketStates.Length)].ToString(),
                    Comment = random.Next(0, 3) == 0 ? "Actualización del estado" : null,
                    Timestamp = ticket.CreatedAt.AddHours(h * random.Next(1, 24))
                });
            }
        }

        context.TicketHistory.AddRange(ticketHistories);
        Console.WriteLine($"✅ {ticketHistories.Count} entradas de historial creadas");

        // Guardar tickets e historial
        context.SaveChanges();

        // Resetear la secuencia de Number para que el próximo ticket use el valor correcto
        context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Tickets\"', 'Number'), (SELECT MAX(\"Number\") FROM \"Tickets\"))");

        // ============================================
        // 6. MOVIMIENTOS DE CAJA (60 movimientos)
        // ============================================
        var movements = new List<BoxMovement>();
        var accounts = new[] { cuentaEfectivo, cuentaBanco, cuentaMercadoPago };
        var movementTypes = Enum.GetValues<MovementType>();
        var paymentMethods = Enum.GetValues<PaymentMethod>();

        var incomeConcepts = new[] {
            "Cobro trabajo realizado", "Seña 50%", "Saldo final", "Cobro al contado",
            "Transferencia cliente", "Pago MercadoPago", "Cobro efectivo", "Anticipo 30%"
        };

        var expenseConcepts = new[] {
            "Compra aluminio", "Compra vidrios", "Compra herrajes", "Pago servicios",
            "Pago sueldos", "Compra herramientas", "Mantenimiento taller", "Alquiler local",
            "Impuestos", "Seguros", "Publicidad", "Transporte materiales"
        };

        // Calcular balances iniciales
        decimal efectivoBalance = 50000;
        decimal bancoBalance = 200000;
        decimal mpBalance = 30000;

        for (int i = 0; i < 60; i++)
        {
            var daysAgo = random.Next(0, 60);
            var type = movementTypes[random.Next(movementTypes.Length)];
            var account = accounts[random.Next(accounts.Length)];
            var method = paymentMethods[random.Next(paymentMethods.Length)];

            // Ajustar método según cuenta
            if (account == cuentaEfectivo) method = PaymentMethod.Efectivo;
            else if (account == cuentaBanco) method = PaymentMethod.Transferencia;
            else method = random.Next(0, 2) == 0 ? PaymentMethod.Transferencia : PaymentMethod.Tarjeta;

            var amount = type == MovementType.Ingreso
                ? random.Next(5000, 300000)
                : random.Next(3000, 150000);

            // Actualizar balances
            if (type == MovementType.Ingreso)
            {
                if (account == cuentaEfectivo) efectivoBalance += amount;
                else if (account == cuentaBanco) bancoBalance += amount;
                else mpBalance += amount;
            }
            else
            {
                if (account == cuentaEfectivo) efectivoBalance -= amount;
                else if (account == cuentaBanco) bancoBalance -= amount;
                else mpBalance -= amount;
            }

            var hasTicket = type == MovementType.Ingreso && random.Next(0, 3) == 0;
            var concept = type == MovementType.Ingreso
                ? incomeConcepts[random.Next(incomeConcepts.Length)]
                : expenseConcepts[random.Next(expenseConcepts.Length)];

            movements.Add(new BoxMovement
            {
                CompanyId = company.Id,
                Id = Guid.NewGuid(),
                Number = i + 1,
                AccountId = account.Id,
                TicketId = hasTicket ? tickets[random.Next(tickets.Count)].Id : null,
                UserId = users[random.Next(users.Length)].Id,
                Type = type,
                Amount = amount,
                Method = method,
                Concept = hasTicket ? $"{concept} - Ticket #{random.Next(1, 41)}" : concept,
                VoucherNumber = type == MovementType.Ingreso ? $"REC-{i + 1:D4}" : $"FAC-{i + 1:D4}",
                Observations = random.Next(0, 5) == 0 ? "Observación adicional" : null,
                MovementDate = DateTime.UtcNow.AddDays(-daysAgo).AddHours(random.Next(8, 20)),
                RegisteredAt = DateTime.UtcNow.AddDays(-daysAgo).AddHours(random.Next(8, 20))
            });
        }

        // Actualizar balances finales de cuentas
        cuentaEfectivo.CurrentBalance = Math.Max(0, efectivoBalance);
        cuentaBanco.CurrentBalance = Math.Max(0, bancoBalance);
        cuentaMercadoPago.CurrentBalance = Math.Max(0, mpBalance);

        context.BoxMovements.AddRange(movements);
        Console.WriteLine("✅ 60 movimientos de caja creados");

        // ============================================
        // GUARDAR TODO
        // ============================================
        context.SaveChanges();

        Console.WriteLine("🎉 Seeding completado exitosamente!");
        Console.WriteLine($"   - Empresas: 3 (Sagitario, Vidrios Norte, AlumCor)");
        Console.WriteLine($"   - Usuarios: 7 (5 en Sagitario, 1 por empresa en las otras)");
        Console.WriteLine($"   - Clientes: 50 (en Sagitario)");
        Console.WriteLine($"   - Cuentas: 3 (en Sagitario)");
        Console.WriteLine($"   - Tickets: 40 (en Sagitario)");
        Console.WriteLine($"   - Historial: {ticketHistories.Count} (en Sagitario)");
        Console.WriteLine($"   - Movimientos: 60 (en Sagitario)");
        Console.WriteLine($"   - Saldo Total Sagitario: ${cuentaEfectivo.CurrentBalance + cuentaBanco.CurrentBalance + cuentaMercadoPago.CurrentBalance:N0}");
    }

    private static void EnsureSubscriptions(ProxarDbContext context)
    {
        try
        {
            // Crear suscripciones para empresas que no las tengan
            var companiesWithoutSubscription = context.Companies
                .Where(c => !context.Subscriptions.Any(s => s.CompanyId == c.Id))
                .ToList();

            if (!companiesWithoutSubscription.Any())
            {
                Console.WriteLine("✅ Todas las empresas tienen suscripciones");
                return;
            }

            var subscriptionsToAdd = new List<Subscription>();
            var paymentsToAdd = new List<SubscriptionPayment>();

            foreach (var company in companiesWithoutSubscription)
            {
                var now = DateTime.UtcNow;
                var periodStart = now.AddDays(-10);
                var periodEnd = now.AddDays(20);

                // Crear suscripción activa para empresas existentes
                var subscription = new Subscription
                {
                    Id = Guid.NewGuid(),
                    CompanyId = company.Id,
                    Plan = SubscriptionPlan.Basic,
                    Status = SubscriptionStatus.Active,
                    MonthlyFee = 29999m,
                    IsOnTrial = false,
                    CurrentPeriodStart = periodStart,
                    CurrentPeriodEnd = periodEnd,
                    NextBillingDate = periodEnd.AddDays(1),
                    FailedPaymentAttempts = 0,
                    LastSuccessfulPaymentAt = periodStart,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                subscriptionsToAdd.Add(subscription);

                // Crear pago de ejemplo asociado a la suscripción
                paymentsToAdd.Add(new SubscriptionPayment
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    CompanyId = company.Id,
                    Amount = subscription.MonthlyFee,
                    Currency = "ARS",
                    Status = PaymentStatus.Success,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    MercadoPagoPaymentId = $"seed-payment-{company.Slug}",
                    MercadoPagoStatus = "approved",
                    CompletedAt = periodStart,
                    CreatedAt = periodStart
                });
            }

            context.Subscriptions.AddRange(subscriptionsToAdd);
            context.SubscriptionPayments.AddRange(paymentsToAdd);
            context.SaveChanges();

            Console.WriteLine($"✅ Creadas {subscriptionsToAdd.Count} suscripciones activas para empresas existentes");
            Console.WriteLine($"✅ Creados {paymentsToAdd.Count} pagos de ejemplo para suscripciones existentes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error creando suscripciones: {ex.Message}");
        }
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
