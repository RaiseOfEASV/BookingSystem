# Observability Stack

Centralized logging and distributed tracing for all HappyHeadlines services.
Run once — every service ships its logs and traces here.

---

## What's included

| Container | Image | Purpose |
|-----------|-------|---------|
| `seq` | `datalust/seq:latest` | Structured log aggregation and search UI |
| `jaeger` | `jaegertracing/all-in-one:latest` | Distributed trace collection and UI |

---

## Architecture

```
                        happyheadlines-observability (Docker network)
                        ┌───────────────────────────────────────────┐
                        │                                           │
                        │   seq        (port 80 inside network)     │
                        │   jaeger     (port 4317 inside network)   │
                        │                                           │
                        └───────────────────────────────────────────┘
                                │                       │
             Serilog sink       │                       │  OTLP gRPC exporter
             (structured logs)  │                       │  (trace spans)
                                │                       │
                        draft-service            profanity-service
                        (any service that        (any service that
                         joins the network)       joins the network)
```

Logs and traces are **correlated by `TraceId`** — every log line produced during a request carries the same `TraceId` as the spans in Jaeger.

---

## Ports

| Service | Host port | URL |
|---------|-----------|-----|
| Seq UI | `5341` | http://localhost:5341 |
| Jaeger UI | `16686` | http://localhost:16686 |
| Jaeger OTLP gRPC | `4317` | (used by services, not a browser URL) |

---

## Setup

### 1. Create the env file

```bash
cp .env.example .env
```

Edit `.env` and set a strong admin password for Seq:

```
SEQ_ADMIN_PASSWORD=your_password_here
```

### 2. Start the stack

```bash
docker compose up -d
```

The `happyheadlines-observability` Docker network is created automatically on first start.

### 3. (Optional) Create a Seq API key for each service

After the stack is running, open the Seq UI at http://localhost:5341 and log in.

Navigate to **Settings → API Keys → Add API Key** (role: `Ingest`).
Copy the generated token into the `SEQ_API_KEY` variable in each service's `.env` file.

Ingestion still works without an API key (open ingestion), but a per-service key lets you
filter logs by key and revoke access individually.

---

## Connecting a service

In the service's `docker-compose.yml`:

```yaml
services:
  your-service:
    environment:
      Seq__ServerUrl: "http://seq:80"
      Seq__ApiKey: ${SEQ_API_KEY}
      Otlp__Endpoint: "http://jaeger:4317"
    networks:
      - default        # service's own internal network (DB, etc.)
      - observability  # shared observability network

networks:
  observability:
    external: true
    name: happyheadlines-observability
```

The application code does not change — `HappyHeadlines.Monitoring` reads `Seq__ServerUrl` and `Otlp__Endpoint` from configuration and activates the relevant sink/exporter automatically when they are set.

---

## Stopping

```bash
docker compose down          # stop containers, keep volumes (logs/traces preserved)
docker compose down -v       # stop containers and delete volumes (wipes all data)
```
