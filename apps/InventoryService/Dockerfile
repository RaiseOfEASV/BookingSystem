# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files first for layer-cached restore
COPY apps/InventoryService/InventoryService.sln                                                                 apps/InventoryService/
COPY apps/InventoryService/src/InventoryService.Domain/InventoryService.Domain.csproj                           apps/InventoryService/src/InventoryService.Domain/
COPY apps/InventoryService/src/InventoryService.Application/InventoryService.Application.csproj                 apps/InventoryService/src/InventoryService.Application/
COPY apps/InventoryService/src/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj           apps/InventoryService/src/InventoryService.Infrastructure/
COPY apps/InventoryService/src/InventoryService.API/InventoryService.API.csproj                                 apps/InventoryService/src/InventoryService.API/

# Copy package csproj files (referenced by the solution)
COPY packages/SharedContracts/SharedContracts.csproj         packages/SharedContracts/
COPY packages/MessageClient/MessageClient.csproj             packages/MessageClient/
COPY packages/Exceptions/Exceptions/Exceptions.csproj        packages/Exceptions/Exceptions/
COPY packages/VaultClient/VaultClient.csproj                 packages/VaultClient/

RUN dotnet restore apps/InventoryService/InventoryService.sln

# Copy the rest of the source and publish
COPY apps/InventoryService/src/ apps/InventoryService/src/
COPY packages/                  packages/

RUN dotnet publish apps/InventoryService/src/InventoryService.API/InventoryService.API.csproj \
    -c Release \
    -o /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InventoryService.API.dll"]
