# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files first for layer-cached restore
COPY apps/BookingService/BookingService.sln                                                                 apps/BookingService/
COPY apps/BookingService/src/BookingService.Domain/BookingService.Domain.csproj                             apps/BookingService/src/BookingService.Domain/
COPY apps/BookingService/src/BookingService.Application/BookingService.Application.csproj                   apps/BookingService/src/BookingService.Application/
COPY apps/BookingService/src/BookingService.Infrastructure/BookingService.Infrastructure.csproj             apps/BookingService/src/BookingService.Infrastructure/
COPY apps/BookingService/src/BookingService.API/BookingService.API.csproj                                   apps/BookingService/src/BookingService.API/

RUN dotnet restore apps/BookingService/BookingService.sln

# Copy the rest of the source and publish
COPY apps/BookingService/src/ apps/BookingService/src/

RUN dotnet publish apps/BookingService/src/BookingService.API/BookingService.API.csproj \
    -c Release \
    -o /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BookingService.API.dll"]
