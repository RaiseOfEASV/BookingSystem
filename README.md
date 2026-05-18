# BookingSystem

A microservices-based resource booking platform supporting hotel rooms, meeting rooms, cinema seats, event tickets, restaurant tables, sports courts, parking spaces, and equipment rental.

## Architecture Diagram

> [View interactive diagram on Excalidraw](https://excalidraw.com/#json=SemlnUAG9Hr-hNIJJkXLo,1OkLyIRugyZymon9h8pcsg)

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
                     │  SQL Server  │ │  SQL Server    │ │  SQL Server  │
                     └──────────────┘ └───────────────┘ └──────────────┘

                     ───────────────── Messaging (RabbitMQ) ─────────────────
                                         MessageClient
                                         SharedContracts
                                    Saga Commands / Compensations
```

---

## Services

### UserService — `apps/UserService`
Manages customer and staff profiles.

| Endpoint | Description |
|---|---|
| `GET /api/users` | List all users |
| `GET /api/users/{id}` | Get user by ID |
| `GET /api/users/email/{email}` | Look up by email |
| `POST /api/users` | Register new user |
| `PUT /api/users/{id}` | Update profile & address |
| `PATCH /api/users/{id}/status` | Change status (Active / Inactive / Suspended) |
| `DELETE /api/users/{id}` | Delete user |

**Domain:** `User`, `Address` (owned), `UserRole` (Customer / Staff / Admin), `UserStatus`

---

### InventoryService — `apps/InventoryService`
Manages bookable resources and their availability.

| Endpoint | Description |
|---|---|
| `GET /api/resources?type=HotelRoom` | List resources (optional type filter) |
| `GET /api/resources/{id}` | Get resource by ID |
| `GET /api/resources/{id}/availability?startTime=&endTime=` | Check available units |
| `POST /api/resources` | Create resource |
| `PUT /api/resources/{id}` | Update resource |
| `DELETE /api/resources/{id}` | Delete resource |

**Domain:** `Resource`, `ResourceType` enum

```
ResourceType: HotelRoom | MeetingRoom | CinemaSeat | EventTicket
              RestaurantTable | SportsCourt | ParkingSpace | Equipment
```

---

### BookingService — `apps/BookingService`
Manages reservations. Each booking supports multiple resource line items (e.g. 2 cinema tickets + 1 parking space in one booking).

| Endpoint | Description |
|---|---|
| `GET /api/bookings` | List all bookings |
| `GET /api/bookings/{id}` | Get booking by ID |
| `GET /api/bookings/customer/{customerId}` | Customer's bookings |
| `GET /api/bookings/resource/{resourceId}` | Bookings for a resource |
| `POST /api/bookings` | Create booking (1 or more items) |
| `PUT /api/bookings/{id}` | Replace all items, recalculate total |
| `PATCH /api/bookings/{id}/cancel` | Cancel booking |
| `DELETE /api/bookings/{id}` | Delete booking |

**Domain:** `Booking` → `BookingItem` (1-to-many), `BookingStatus`, `ResourceType`

```
Booking
 ├── Id, CustomerId, Status, TotalPrice, Notes
 └── Items[]
      ├── ResourceId, ResourceType (enum), ResourceLocation
      ├── StartTime, EndTime
      ├── Quantity, UnitPrice, SubTotal
```

**BookingStatus:** `Pending` → `Confirmed` → `Completed` / `Cancelled`

---

## Clean Architecture

Every service follows the same four-layer structure:

```
src/
├── <Service>.Domain          # Entities, enums — no dependencies
├── <Service>.Application     # Interfaces, DTOs, service logic
├── <Service>.Infrastructure  # EF Core DbContext, repositories, DI registration
└── <Service>.API             # Controllers, Program.cs, appsettings.json
```

**Dependency rule:** `API` → `Infrastructure` → `Application` → `Domain`

---

## Shared Packages — `packages/`

### SharedContracts
Cross-service message contracts for the Saga orchestration pattern.

```
packages/SharedContracts/src/Saga/
├── Commands/
│   ├── CreateBookingCommand.cs
│   ├── InitiatePaymentCommand.cs
│   └── LaunchNotificationCommand.cs
└── Compensate/
    ├── ReleaseSeatCommand.cs
    └── UpdateBookingStatusToCancelledCommand.cs
```

All commands carry `CommandId` (unique per message) and `CorrelationId` (shared across the full saga) for distributed tracing.

### MessageClient
RabbitMQ abstraction built on EasyNetQ. Provides `IMessageClient` for publishing and `IMessageBackgroundService` for subscribing.

---

## Saga Pattern

The booking flow is orchestrated as a saga:

```
CreateBookingCommand
        │
        ▼
  BookingService  ──(success)──▶  InitiatePaymentCommand
        │                                  │
        │                         (success)▼
        │                         LaunchNotificationCommand
        │                           (BookingConfirmed)
        │
        └──(failure)──▶  ReleaseSeatCommand
                         UpdateBookingStatusToCancelledCommand
                         LaunchNotificationCommand
                           (BookingCancelled / PaymentFailed)
```

---

## Tech Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core |
| ORM | Entity Framework Core 9 |
| Database | SQL Server 2022 |
| Messaging | RabbitMQ via EasyNetQ |
| Serialisation | System.Text.Json |
| Reverse proxy | Nginx |
| Containerisation | Docker / Docker Compose |
| API docs | Swagger (Swashbuckle) |

---

## Repository Structure

```
BookingSystem/
├── apps/
│   ├── BookingService/
│   │   ├── docker-compose.yml
│   │   ├── Dockerfile
│   │   ├── nginx/default.conf
│   │   └── src/
│   ├── InventoryService/
│   │   ├── docker-compose.yml
│   │   ├── Dockerfile
│   │   ├── nginx/default.conf
│   │   └── src/
│   └── UserService/
│       ├── Dockerfile
│       └── src/
├── packages/
│   ├── MessageClient/
│   └── SharedContracts/
├── nginx/                  # Root-level nginx config (global compose)
├── docker-compose.yml      # Runs all three services together
└── .env.example
```

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

Services are available via Nginx on `http://localhost:5000`:

```
http://localhost:5000/api/users
http://localhost:5000/api/resources
http://localhost:5000/api/bookings
```

### Run a single service

```bash
cd apps/BookingService
cp .env.example .env
docker-compose up --build
# Nginx on http://localhost:5001
```

```bash
cd apps/InventoryService
cp .env.example .env
docker-compose up --build
# Nginx on http://localhost:5002
```

### Run locally without Docker

```bash
cd apps/BookingService/src/BookingService.API
dotnet run
# Swagger at https://localhost:5001/swagger
```

### EF Core Migrations

```bash
cd apps/BookingService/src/BookingService.API
dotnet ef migrations add InitialCreate --project ../BookingService.Infrastructure
dotnet ef database update
```

Repeat for `InventoryService` and `UserService`.
