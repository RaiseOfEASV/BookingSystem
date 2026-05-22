# InventoryService

Manages bookable resources (events, venues, seats) and their availability.

## API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/resources` | List resources (optional `?type=` filter) |
| `GET` | `/api/resources/{id}` | Get resource by ID |
| `GET` | `/api/resources/{id}/availability` | Check available seats (`?startTime=&endTime=`) |
| `POST` | `/api/resources` | Create resource |
| `PUT` | `/api/resources/{id}` | Update resource |
| `DELETE` | `/api/resources/{id}` | Delete resource |

**ResourceType:** `HotelRoom` | `MeetingRoom` | `CinemaSeat` | `EventTicket` | `RestaurantTable` | `SportsCourt` | `ParkingSpace` | `Equipment`

## Domain

```
Event
 ├── EventName, Date, TotalSeats
 └── Venue
      ├── VenueName, VenueType
      └── Address
Seat
 ├── SeatRow, SeatSection
 └── Status: Available | Reserved | Booked
```

## Saga participation

InventoryService consumes `ReserveSeatCommand` (published by BookingService during the booking saga) and emits `SeatStatusChangedEvent` on success or failure.

## Infrastructure

- **Primary DB:** SQL Server 2022 (inventory data, Flyway migrations in `db/migrations/InventorySqlServer/`)
- **Messages DB:** PostgreSQL 16 (outbox / inbox for messaging, `db/migrations/InventoryMessagesPostgresDb/`)
- **Background worker:** `ReserveSeatWorker` polls the messages DB and processes incoming commands

## Running

**Local (no Docker):**
```bash
cd src/InventoryService.API
dotnet run
# Swagger: https://localhost:5001/swagger
```

**Docker (standalone):**
```bash
cp .env.example .env
docker-compose up --build
# Nginx on http://localhost:5002
```
