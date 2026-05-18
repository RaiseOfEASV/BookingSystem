# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files first for layer-cached restore
COPY apps/UserService/UserService.sln                                                         apps/UserService/
COPY apps/UserService/src/UserService.Domain/UserService.Domain.csproj                         apps/UserService/src/UserService.Domain/
COPY apps/UserService/src/UserService.Application/UserService.Application.csproj               apps/UserService/src/UserService.Application/
COPY apps/UserService/src/UserService.Infrastructure/UserService.Infrastructure.csproj         apps/UserService/src/UserService.Infrastructure/
COPY apps/UserService/src/UserService.API/UserService.API.csproj                               apps/UserService/src/UserService.API/

RUN dotnet restore apps/UserService/UserService.sln

# Copy the rest of the source and publish
COPY apps/UserService/src/ apps/UserService/src/

RUN dotnet publish apps/UserService/src/UserService.API/UserService.API.csproj \
    -c Release \
    -o /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UserService.API.dll"]
