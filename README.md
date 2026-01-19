# Resource Booking Platform

A resource booking/reservation system built to demonstrate production-oriented
engineering practices: clean architecture, tested domain logic, a documented
concurrency strategy, containerized local development, and CI.

**Status: early scaffold.** Domain model and conflict-prevention logic are the
current focus; see `docs/adr/0001-time-slot-concurrency-strategy.md` for the
concurrency design and the project's issues/board for what's in flight.

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
  ResourceBooking.Api             - ASP.NET Core REST API
tests/
  ResourceBooking.Domain.Tests    - unit tests for domain invariants
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

## Roadmap

- [x] Project scaffolding, ADR-0001, CI skeleton
- [ ] Booking conflict-prevention logic + unit tests
- [ ] REST API: resources CRUD, booking create/cancel, availability query
- [ ] Auth (JWT) + role-based authorization
- [ ] Frontend booking flow + admin dashboard
- [ ] Background reminder notifications, availability caching
- [ ] CI/CD deployment to Azure
