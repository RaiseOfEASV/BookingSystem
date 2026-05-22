# UserService

Manages customer and staff profiles.

## API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/users` | List all users |
| `GET` | `/api/users/{id}` | Get user by ID |
| `GET` | `/api/users/email/{email}` | Look up by email |
| `POST` | `/api/users` | Register new user |
| `PUT` | `/api/users/{id}` | Update profile & address |
| `PATCH` | `/api/users/{id}/status` | Change status |
| `DELETE` | `/api/users/{id}` | Delete user |

**UserStatus:** `Active` / `Inactive` / `Suspended`

## Domain

```
User
 ├── Id, Email, FirstName, LastName
 ├── Role: Customer | Staff | Admin
 ├── Status: Active | Inactive | Suspended
 └── Address (owned entity)
      └── Street, City, Country, PostalCode
```

## Running

**Local (no Docker):**
```bash
cd src/UserService.API
dotnet run
# Swagger: https://localhost:5001/swagger
```

**Docker (standalone):**
```bash
# UserService has no docker-compose of its own — run via the root compose
cd ../..
cp .env.example .env
docker-compose up --build user-api
```
