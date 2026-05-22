# Packages

Shared libraries and infrastructure stacks used across all services.

## MessageClient

RabbitMQ abstraction built on EasyNetQ.

- `IMessageClient` — publish messages
- `IMessageBackgroundService` — subscribe and process messages
- `IMessageHandler<T>` — implement to handle a specific message type
- Registered via `ServiceCollectionExtension.AddMessageClient()`

## SharedContracts

Cross-service message contracts for the saga orchestration pattern.

```
src/saga/
├── commands/
│   ├── CreateBookingCommand.cs
│   ├── ReserveSeatCommand.cs
│   ├── InitiatePaymentCommand.cs
│   ├── ConfirmBookingStatusCommand.cs
│   └── SendNotificationCommand.cs
└── compensate/
    ├── ReleaseSeatCommand.cs
    └── CancelBookingStatusToCancelledCommand.cs
```

All commands carry `CommandId` (unique per message) and `CorrelationId` (shared across the full saga) for distributed tracing.

## infra/

Independent Docker Compose stacks shared across services — start once, used by all.

| Stack | Path | Purpose |
|-------|------|---------|
| Messaging | `infra/messaging/` | RabbitMQ |
| Metrics | `infra/metrics/` | Prometheus + Grafana |
| Observability | `infra/observability/` | Seq (logs) + Jaeger (traces) |

Each stack creates its own Docker network; services join it as `external: true`.
