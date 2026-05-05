# Variables de Entorno para Render.com

## Backend Service Configuration

### Database (REQUIRED)
```
ConnectionStrings__DefaultConnection=postgresql://aguh_admin:OciUqidbvAkY9N2v8zcJgCXJQb8Ci5hG@dpg-d7t6fod7vvec73elsai0-a/proxar
```

### JWT Settings (REQUIRED)
```
JwtSettings__SecretKey=<GENERAR_CLAVE_SEGURA_MINIMO_32_CARACTERES>
JwtSettings__Issuer=ProxarAPI
JwtSettings__Audience=ProxarClient
JwtSettings__ExpirationMinutes=10080
JwtSettings__RefreshTokenExpirationDays=30
```

### Encryption (REQUIRED)
```
Encryption__Key=<GENERAR_CON: openssl rand -hex 32>
```

### CORS (REQUIRED)
```
AllowedOrigins__0=https://tu-frontend.onrender.com
```

### AFIP (OPTIONAL - para testing)
```
Afip__Environment=Testing
Afip__Cuit=
Afip__CertificatePath=
Afip__PrivateKeyPath=
Afip__CertificatePassword=
```

### ASP.NET Core
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

## Build & Start Commands

### Build Command
```bash
dotnet publish ProxarAPI/ProxarAPI.csproj -c Release -o out
```

### Start Command
```bash
cd out && dotnet ProxarAPI.dll
```

## Notas Importantes

1. **JwtSettings__SecretKey**: Generar con `openssl rand -base64 32` o una cadena aleatoria de mínimo 32 caracteres
2. **Encryption__Key**: Generar con `openssl rand -hex 32` (debe ser exactamente 64 caracteres hexadecimales)
3. **AllowedOrigins**: Actualizar con la URL real del frontend cuando esté deployado
4. **$PORT**: Render asigna automáticamente el puerto, usa la variable `$PORT`
5. **Base de datos**: Ya está creada y las migraciones aplicadas ✅
