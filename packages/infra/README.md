# Shared Infrastructure Stacks

Independent Docker Compose stacks — start once, shared across all services.

Each stack declares a named Docker network; each service's compose file joins it as `external: true`.

## Stacks

| Stack | Path | Purpose |
|-------|------|---------|
| Messaging | `messaging/` | RabbitMQ message broker |
| Metrics | `metrics/` | Prometheus + Grafana |
| Observability | `observability/` | Seq (structured logs) + Jaeger (distributed traces) |

## Running

Start each stack before the services that depend on it:

```bash
cd messaging && docker compose up -d
cd metrics   && docker compose up -d
cd observability && cp .env.example .env && docker compose up -d
```
