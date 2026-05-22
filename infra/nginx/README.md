# Nginx — Global Reverse Proxy

Routes all inbound traffic on `localhost:5000` to the three backend services based on URL prefix.

## Routing

| Prefix | Upstream |
|--------|----------|
| `/api/bookings` | `booking-api:8080` |
| `/api/resources` | `inventory-api:8080` |
| `/api/users` | `user-api:8080` |

Used exclusively by the root `docker-compose.yml` (the "run all services together" mode). Each service also ships its own nginx config for single-service mode — see `apps/<Service>/nginx/`.
