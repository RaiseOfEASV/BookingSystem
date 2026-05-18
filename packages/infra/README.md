# Infrastructure

Shared infrastructure stacks for the HappyHeadlines platform.
Each subfolder is an independent Docker Compose stack that can be started once and shared across all services.

---

## Structure

```
infra/
└── observability/   # Centralized logging (Seq) + distributed tracing (Jaeger)
```

---

## How it works

Services connect to infra stacks through a shared Docker bridge network.
Each infra stack declares a named network; each application compose file joins it as `external: true`.

```
infra/observability  ──creates──►  happyheadlines-observability  (Docker network)
                                            │
              ┌─────────────────────────────┴──────────────────────────┐
              │                                                         │
        draft-service                                        profanity-service
        (joins network)                                      (joins network)
```

---

## Stacks

| Stack | Path | Purpose |
|-------|------|---------|
| Observability | `infra/observability/` | Seq (logs) + Jaeger (traces) |

---

## Running

Start each stack before the services that depend on it:

```bash
# Observability
cd infra/observability
cp .env.example .env   # set SEQ_ADMIN_PASSWORD
docker compose up -d
```

Services can be started in any order after the relevant infra stack is running.
