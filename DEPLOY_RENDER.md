# Deploy Backend en Render.com - Checklist

## ✅ Completado

1. **Base de datos PostgreSQL creada en Render**
   - Host: `dpg-d7t6fod7vvec73elsai0-a.virginia-postgres.render.com`
   - Database: `proxar`
   - Username: `aguh_admin`
   - ✅ 14 tablas creadas + migraciones aplicadas

2. **Build de producción verificado**
   - ✅ Compila sin errores
   - ✅ Modo Release funcionando

## 📋 Pasos para Deploy en Render

### 1. Crear Web Service en Render

1. Ir a [Render Dashboard](https://dashboard.render.com)
2. Click en **"New +"** → **"Web Service"**
3. Conectar repositorio: `LilAguh/Proxar-Back`
4. Configuración del servicio:
   - **Name**: `proxar-api` (o el que prefieras)
   - **Region**: `Virginia (US East)` (misma que la BD)
   - **Branch**: `main`
   - **Root Directory**: (dejar vacío)
   - **Environment**: `Docker`
   - **Dockerfile Path**: `Dockerfile` (Render lo detecta automáticamente)
   - **Instance Type**: Free o Starter (según necesites)

**IMPORTANTE**: Render detecta automáticamente el Dockerfile y lo usa para el build. No necesitás configurar Build Command ni Start Command.

### 2. Configurar Variables de Entorno

En la sección **Environment** del servicio, agregar:

#### OBLIGATORIAS:

```bash
# Database (Render usa DATABASE_URL por defecto)
DATABASE_URL=postgresql://aguh_admin:OciUqidbvAkY9N2v8zcJgCXJQb8Ci5hG@dpg-d7t6fod7vvec73elsai0-a/proxar

# JWT (usa las claves generadas abajo)
JwtSettings__SecretKey=Khm94jUUKc+rI5Wqfakn9Lju1dqyEdApOU6mP4OEz8CajU8F17YJJJoht4mgK6NA
JwtSettings__Issuer=ProxarAPI
JwtSettings__Audience=ProxarClient
JwtSettings__ExpirationMinutes=10080
JwtSettings__RefreshTokenExpirationDays=30

# Encryption (usa la clave generada abajo)
Encryption__Key=2990225773f65d51cafc46949e4ec398c640a6599abc604a2c127b62f61c43c3

# CORS (actualizar cuando tengas la URL del frontend)
AllowedOrigins__0=https://app.proxar.com.ar

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

#### OPCIONALES (AFIP - solo si vas a usar en testing):

```bash
Afip__Environment=Testing
Afip__Cuit=
Afip__CertificatePath=
Afip__PrivateKeyPath=
Afip__CertificatePassword=
```

### 3. Deploy

1. Click en **"Create Web Service"**
2. Render automáticamente:
   - Clonará el repo
   - Ejecutará el build
   - Iniciará la aplicación
   - Te dará una URL tipo: `https://proxar-api.onrender.com`

### 4. Verificar Deploy

Una vez desplegado, probar:

```bash
curl https://TU-URL.onrender.com/health
```

O directamente en el navegador ir a:
```
https://TU-URL.onrender.com/swagger
```

## 🔐 Claves Generadas para Producción

**JWT Secret Key:**
```
Khm94jUUKc+rI5Wqfakn9Lju1dqyEdApOU6mP4OEz8CajU8F17YJJJoht4mgK6NA
```

**Encryption Key:**
```
2990225773f65d51cafc46949e4ec398c640a6599abc604a2c127b62f61c43c3
```

⚠️ **IMPORTANTE**: Estas claves son únicas para producción. NO commitearlas al repositorio.

## 📝 Notas

- **Puerto**: Render asigna automáticamente el puerto vía variable `$PORT`, la app ya lo maneja en Program.cs
- **SSL/HTTPS**: Render lo maneja automáticamente
- **Health Check**: Render hace health checks automáticos
- **Logs**: Disponibles en el dashboard de Render
- **Free Tier**: Se duerme después de 15 min de inactividad, tarda ~30s en despertar

## 🔄 Próximos Pasos

1. ✅ Deployar backend
2. ⏳ Deployar frontend (React en Render o Vercel)
3. ⏳ Actualizar `AllowedOrigins__0` con la URL real del frontend
4. ⏳ Configurar dominio personalizado si lo tenés (app.proxar.com.ar)

## 🐛 Troubleshooting

Si el deploy falla, revisar:
1. **Build Logs** en Render - ver errores de compilación
2. **Runtime Logs** - ver errores al iniciar
3. **Variables de entorno** - verificar que estén bien escritas (doble underscore `__`)
4. **Connection string** - verificar que la BD sea accesible desde el servicio
