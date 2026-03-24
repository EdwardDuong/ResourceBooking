# Resource Booking Platform

A resource booking/reservation system built to demonstrate production-oriented
engineering practices: clean architecture, tested domain logic, a documented
concurrency strategy, containerized local development, and CI.

**Status: availability caching complete.** Computed availability grids are
cached in-process and explicitly invalidated on every booking/cancellation,
so a popular resource doesn't force a full booking-table scan on every page
load, without ever serving stale availability. Background reminders and
CI/CD to Azure are next. See
`docs/adr/0001-time-slot-concurrency-strategy.md` for the concurrency
design, and `frontend/README.md` for the client's structure.

## Problem

Any system that lets multiple users book a shared resource (a room, a piece
of equipment, a time slot with a person) has to guarantee that two people
can't book the same resource for the same time, even when requests race each
other. That guarantee - not the CRUD around it - is the interesting part of
this project.

## Architecture

Clean/onion architecture on the backend:

```
src/
  ResourceBooking.Domain          - entities, invariants, no external deps
  ResourceBooking.Application     - use cases (CQRS via MediatR), validation
  ResourceBooking.Infrastructure  - EF Core persistence, SQL Server
  ResourceBooking.Api             - ASP.NET Core REST API, controllers
tests/
  ResourceBooking.Domain.Tests         - unit tests for domain invariants
  ResourceBooking.Infrastructure.Tests - persistence + concurrency tests (SQLite)
  ResourceBooking.Api.Tests            - HTTP-level integration tests (SQLite)
frontend/                         - React + TypeScript client (see frontend/README.md)
```

## Tech stack

- Backend: .NET 8, ASP.NET Core, EF Core, MediatR, FluentValidation
- Frontend: React + TypeScript (Vite)
- Database: SQL Server
- Infra: Docker Compose (local), GitHub Actions (CI), Azure (deployment target)

## Local development

Prerequisites: .NET 8 SDK, Node.js 18+, Docker.

`appsettings.json` ships with placeholder values for the DB password and JWT
signing key - real values go in user secrets, never committed:

```bash
cd src/ResourceBooking.Api
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=ResourceBooking;User Id=sa;Password=<your-password>;TrustServerCertificate=true;"
dotnet user-secrets set "Jwt:SigningKey" "<32+ random bytes, base64 or plain - anything the placeholder length check will pass>"
```

```bash
# regenerate the solution file and wire up project references locally
./scripts/bootstrap-solution.sh

# start SQL Server for local dev
docker compose up -d sqlserver

# backend
cd src/ResourceBooking.Api
dotnet run

# frontend (separate terminal)
cd frontend
npm install
npm run dev
```

## Testing

```bash
dotnet test
```

Three levels: domain unit tests (no I/O), persistence/concurrency tests
(SQLite), and HTTP integration tests through `WebApplicationFactory` (also
SQLite) - none require a running SQL Server instance, so `dotnet test` works
the same locally and in CI.

## API

| Method | Route                        | Auth           | Description                          |
|--------|------------------------------|----------------|---------------------------------------|
| POST   | `/api/auth/register`         | none           | Create an account (Member), returns a token |
| POST   | `/api/auth/login`             | none           | Returns a token                       |
| GET    | `/api/resources`              | any user       | List active resources                 |
| GET    | `/api/resources/{id}`         | any user       | Get a resource by id                  |
| POST   | `/api/resources`              | Admin          | Create a resource                     |
| DELETE | `/api/resources/{id}`         | Admin          | Deactivate a resource                 |
| POST   | `/api/bookings`                | any user       | Create a booking for yourself         |
| DELETE | `/api/bookings/{id}`           | owner or Admin | Cancel a booking                      |
| GET    | `/api/bookings/availability`   | any user       | Slot availability for a resource/day  |

Send the token from register/login as `Authorization: Bearer <token>`. There
is no client-facing way to register as Admin - seed one directly or promote
via the database; letting registration accept a role would be a privilege
escalation bug.

Errors are returned as `application/problem+json` (see
`GlobalExceptionHandler`): 401 for missing/invalid credentials, 403 for
authenticated-but-not-permitted, 404 for missing entities, 409 for booking
conflicts, inactive-resource bookings and duplicate emails, 400 for
validation failures with a per-field `errors` object.

## Roadmap

- [x] Project scaffolding, ADR-0001, CI skeleton
- [x] Booking conflict-prevention logic + unit tests
- [x] REST API: resources CRUD, booking create/cancel, availability query
- [x] Auth (JWT) + role-based authorization
- [x] Frontend booking flow + admin dashboard
- [x] Availability caching
- [ ] Background reminder notifications
- [ ] CI/CD deployment to Azure (free-tier App Service + free-tier SQL Database)
