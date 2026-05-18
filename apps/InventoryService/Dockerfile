# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files first for layer-cached restore
COPY apps/InventoryService/InventoryService.sln                                                                 apps/InventoryService/
COPY apps/InventoryService/src/InventoryService.Domain/InventoryService.Domain.csproj                           apps/InventoryService/src/InventoryService.Domain/
COPY apps/InventoryService/src/InventoryService.Application/InventoryService.Application.csproj                 apps/InventoryService/src/InventoryService.Application/
COPY apps/InventoryService/src/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj           apps/InventoryService/src/InventoryService.Infrastructure/
COPY apps/InventoryService/src/InventoryService.API/InventoryService.API.csproj                                 apps/InventoryService/src/InventoryService.API/

RUN dotnet restore apps/InventoryService/InventoryService.sln

# Copy the rest of the source and publish
COPY apps/InventoryService/src/ apps/InventoryService/src/

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
