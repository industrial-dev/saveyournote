# ============================================
# Stage 1: Base - Imagen base común
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app

# Copiar archivos de proyecto para restore
COPY ["src/SaveYourNote.Api/SaveYourNote.Api.csproj", "src/SaveYourNote.Api/"]
COPY ["src/SaveYourNote.Application/SaveYourNote.Application.csproj", "src/SaveYourNote.Application/"]
COPY ["src/SaveYourNote.Domain/SaveYourNote.Domain.csproj", "src/SaveYourNote.Domain/"]
COPY ["src/SaveYourNote.Infrastructure/SaveYourNote.Infrastructure.csproj", "src/SaveYourNote.Infrastructure/"]

# Restore de dependencias
RUN dotnet restore "src/SaveYourNote.Api/SaveYourNote.Api.csproj"

# Copiar todo el código fuente
COPY . .

# ============================================
# Stage 2: Development - Para desarrollo local
# ============================================
FROM base AS development

WORKDIR /app/src/SaveYourNote.Api

# Instalar dotnet-watch para hot reload
RUN dotnet tool install --global dotnet-watch
ENV PATH="${PATH}:/root/.dotnet/tools"

# Exponer puerto
EXPOSE 5001

# Comando para ejecutar con hot reload
CMD ["dotnet", "watch", "run", "--non-interactive", "--urls", "http://0.0.0.0:5001"]

# ============================================
# Stage 3: Build - Compilar para producción
# ============================================
FROM base AS build

WORKDIR /app/src/SaveYourNote.Api

# Build en Release mode
RUN dotnet build "SaveYourNote.Api.csproj" -c Release -o /app/build

# ============================================
# Stage 4: Publish - Publicar aplicación
# ============================================
FROM build AS publish

RUN dotnet publish "SaveYourNote.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ============================================
# Stage 5: Production - Imagen final optimizada
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production

WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Crear usuario no privilegiado
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app

# Copiar artefactos publicados
COPY --from=publish --chown=appuser:appuser /app/publish .

# Cambiar a usuario no privilegiado
USER appuser

# Exponer puerto
EXPOSE 5001

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:5001 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

# Comando de inicio
ENTRYPOINT ["dotnet", "SaveYourNote.Api.dll"]
