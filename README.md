# Resource Booking Platform

A resource booking/reservation system built to demonstrate production-oriented
engineering practices: clean architecture, tested domain logic, a documented
concurrency strategy, containerized local development, and CI.

**Status: core REST API complete.** Resources and bookings are fully wired up
end to end - domain, persistence, MediatR use cases, controllers, and three
levels of automated tests (domain, persistence/concurrency, HTTP). Auth is
next. See `docs/adr/0001-time-slot-concurrency-strategy.md` for the
concurrency design.

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
frontend/                         - React + TypeScript client
```

## Tech stack

- Backend: .NET 8, ASP.NET Core, EF Core, MediatR, FluentValidation
- Frontend: React + TypeScript (Vite)
- Database: SQL Server
- Infra: Docker Compose (local), GitHub Actions (CI), Azure (deployment target)

## Local development

Prerequisites: .NET 8 SDK, Node.js 18+, Docker.

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

| Method | Route                              | Description                          |
|--------|-------------------------------------|---------------------------------------|
| GET    | `/api/resources`                    | List active resources                 |
| GET    | `/api/resources/{id}`               | Get a resource by id                  |
| POST   | `/api/resources`                    | Create a resource                     |
| DELETE | `/api/resources/{id}`               | Deactivate a resource                 |
| POST   | `/api/bookings`                     | Create a booking                      |
| DELETE | `/api/bookings/{id}`                | Cancel a booking                      |
| GET    | `/api/bookings/availability`        | Slot availability for a resource/day  |

Errors are returned as `application/problem+json` (see
`GlobalExceptionHandler`): 404 for missing entities, 409 for booking
conflicts and inactive-resource bookings, 400 for validation failures with a
per-field `errors` object.

## Roadmap

- [x] Project scaffolding, ADR-0001, CI skeleton
- [x] Booking conflict-prevention logic + unit tests
- [x] REST API: resources CRUD, booking create/cancel, availability query
- [ ] Auth (JWT) + role-based authorization
- [ ] Frontend booking flow + admin dashboard
- [ ] Background reminder notifications, availability caching
- [ ] CI/CD deployment to Azure
