# BookingSystem

A microservices-based resource booking platform supporting hotel rooms, meeting rooms, cinema seats, event tickets, restaurant tables, sports courts, parking spaces, and equipment rental.

> [View interactive architecture diagram on Excalidraw](https://excalidraw.com/#json=SemlnUAG9Hr-hNIJJkXLo,1OkLyIRugyZymon9h8pcsg)

---

## Services

| Service | Path | Description |
|---------|------|-------------|
| [UserService](apps/UserService/README.md) | `apps/UserService/` | Customer & staff profiles |
| [InventoryService](apps/InventoryService/README.md) | `apps/InventoryService/` | Bookable resources & availability |
| [BookingService](apps/BookingService/README.md) | `apps/BookingService/` | Reservations & saga orchestration |

---

## Packages

See [`packages/README.md`](packages/README.md) for `MessageClient`, `SharedContracts`, and shared infra stacks (RabbitMQ, Prometheus, Seq, Jaeger).

---

See the README in each service folder for per-service instructions.
