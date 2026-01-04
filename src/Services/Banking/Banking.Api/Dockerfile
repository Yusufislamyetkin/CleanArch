# Clean Architecture Banking API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for health checks
RUN apk add --no-cache curl

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file
COPY ["Enterprise.Banking.sln", "."]

# Copy project files for Banking.Api and its dependencies
COPY ["src/BuildingBlocks/Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/Domain/"]
COPY ["src/BuildingBlocks/Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/Application/"]
COPY ["src/BuildingBlocks/Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]
COPY ["src/BuildingBlocks/WebHost/BuildingBlocks.WebHost.csproj", "src/BuildingBlocks/WebHost/"]
COPY ["src/Services/Banking/Banking.Domain/Banking.Domain.csproj", "src/Services/Banking/Banking.Domain/"]
COPY ["src/Services/Banking/Banking.Application/Banking.Application.csproj", "src/Services/Banking/Banking.Application/"]
COPY ["src/Services/Banking/Banking.Infrastructure/Banking.Infrastructure.csproj", "src/Services/Banking/Banking.Infrastructure/"]
COPY ["src/Services/Banking/Banking.Api/Banking.Api.csproj", "src/Services/Banking/Banking.Api/"]

# Restore dependencies
RUN dotnet restore "Enterprise.Banking.sln"

# Copy everything else
COPY . .

# Build the API project
WORKDIR "/src/src/Services/Banking/Banking.Api"
RUN dotnet build "Banking.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Banking.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "Banking.Api.dll"]
