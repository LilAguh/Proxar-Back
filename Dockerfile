# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos .csproj
COPY ["ProxarAPI/ProxarAPI.csproj", "ProxarAPI/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["Exceptions/Exceptions.csproj", "Exceptions/"]
COPY ["Config/Config.csproj", "Config/"]

# Restore
RUN dotnet restore "ProxarAPI/ProxarAPI.csproj"

# Copiar todo el código
COPY . .

# Build
WORKDIR "/src/ProxarAPI"
RUN dotnet build "ProxarAPI.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ProxarAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render usa la variable PORT
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE ${PORT}

ENTRYPOINT ["dotnet", "ProxarAPI.dll"]
