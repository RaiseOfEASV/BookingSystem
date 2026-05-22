# BookingSystem

A microservices-based resource booking platform supporting hotel rooms, meeting rooms, cinema seats, event tickets, restaurant tables, sports courts, parking spaces, and equipment rental.

> [View interactive architecture diagram on Excalidraw](https://excalidraw.com/#json=SemlnUAG9Hr-hNIJJkXLo,1OkLyIRugyZymon9h8pcsg)

---

## System Overview

```
                        ┌─────────────────────────────────────────────┐
                        │                  Nginx                       │
                        │             localhost:5000                    │
                        └──────┬──────────────┬──────────────┬─────────┘
                               │              │              │
                    /api/users │   /api/res.. │  /api/book.. │
                               ▼              ▼              ▼
                     ┌──────────────┐ ┌───────────────┐ ┌──────────────┐
                     │  UserService │ │InventoryService│ │BookingService│
                     │  :8080       │ │ :8080          │ │ :8080        │
                     └──────┬───────┘ └───────┬────────┘ └──────┬───────┘
                            │                 │                  │
                            ▼                 ▼                  ▼
                     ┌──────────────┐ ┌───────────────┐ ┌──────────────┐
                     │ UserServiceDb│ │InventoryServDb│ │BookingServDb │
                     │  SQL Server  │ │  SQL Server    │ │  PostgreSQL  │
                     └──────────────┘ └───────────────┘ └──────────────┘

                     ───────────────── Messaging (RabbitMQ) ─────────────────
                                         MessageClient · SharedContracts
                                         Saga Commands / Compensations
```

---

## Services

| Service | Path | Description |
|---------|------|-------------|
| [UserService](apps/UserService/README.md) | `apps/UserService/` | Customer & staff profiles |
| [InventoryService](apps/InventoryService/README.md) | `apps/InventoryService/` | Bookable resources & availability |
| [BookingService](apps/BookingService/README.md) | `apps/BookingService/` | Reservations & saga orchestration |

Each service follows a four-layer Clean Architecture: `Domain` → `Application` → `Infrastructure` → `API`.

---

## Packages

See [`packages/README.md`](packages/README.md) for `MessageClient`, `SharedContracts`, and shared infra stacks (RabbitMQ, Prometheus, Seq, Jaeger).

---

## Tech Stack

| Concern | Technology |
|---------|------------|
| Runtime | .NET 9 / ASP.NET Core |
| ORM | Entity Framework Core 9 |
| Database | SQL Server 2022 / PostgreSQL 16 |
| Messaging | RabbitMQ via EasyNetQ |
| Cache | Redis 7 |
| Reverse proxy | Nginx (config in [`infra/nginx/`](infra/nginx/)) |
| Containerisation | Docker / Docker Compose |
| API docs | Swagger (Swashbuckle) |

---

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run all services together

```bash
cp .env.example .env
# Set SA_PASSWORD in .env

docker network create bookingsystem-services
docker-compose up --build
```

All services are available via Nginx at `http://localhost:5000`:

```
http://localhost:5000/api/users
http://localhost:5000/api/resources
http://localhost:5000/api/bookings
```

### Run a single service

See the README in each service folder for per-service instructions.
