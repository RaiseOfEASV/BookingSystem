# BookingService

Manages reservations. Each booking supports multiple resource line items (e.g. 2 cinema tickets + 1 parking space in one booking).

## API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/bookings` | List all bookings |
| `GET` | `/api/bookings/{id}` | Get booking by ID |
| `GET` | `/api/bookings/customer/{customerId}` | Customer's bookings |
| `GET` | `/api/bookings/resource/{resourceId}` | Bookings for a resource |
| `POST` | `/api/bookings` | Create booking (1 or more items) |
| `PUT` | `/api/bookings/{id}` | Replace all items, recalculate total |
| `PATCH` | `/api/bookings/{id}/cancel` | Cancel booking |
| `DELETE` | `/api/bookings/{id}` | Delete booking |

**BookingStatus:** `Pending` → `Confirmed` → `Completed` / `Cancelled`

## Domain

```
Booking
 ├── Id, CustomerId, Status, TotalPrice, Notes
 └── Items[]
      ├── ResourceId, ResourceType, ResourceLocation
      ├── StartTime, EndTime
      └── Quantity, UnitPrice, SubTotal
```

## Saga

BookingService is the saga orchestrator. On booking creation it publishes `CreateBookingCommand` and coordinates the full flow via `BookingSagaOrchestrator`:

```
CreateBookingCommand
    │
    ▼
BookingService ──(success)──▶ ReserveSeatCommand → InitiatePaymentCommand → SendNotificationCommand
    │
    └──(failure)──▶ ReleaseSeatCommand + CancelBookingStatusToCancelledCommand
```

Saga state is persisted in `booking_sagas` via `IBookingSagaRepository`.

## Infrastructure

- **Database:** PostgreSQL 16 (via Flyway migrations in `db/migrations/`)
- **Cache:** Redis 7 (`ISeatAvailabilityCache`)
- **Messaging:** RabbitMQ via `MessageClient`
- **Outbox:** `OutboxWorker` background service for reliable event publishing

## Running

**Local (no Docker):**
```bash
cd src/BookingService.API
dotnet run
# Swagger: https://localhost:5001/swagger
```

**Docker (standalone with Postgres + Redis):**
```bash
cp .env.example .env
docker-compose up --build
# Nginx on http://localhost:5001
```

**EF / Migrations:** BookingService uses raw Flyway SQL migrations, not EF migrations.
