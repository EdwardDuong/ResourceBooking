# ADR-0001: Time-slot concurrency strategy for booking conflicts

## Status
Accepted (for MVP scope)

## Context
The core correctness requirement of this system is: two overlapping bookings
for the same resource must never both succeed, even under concurrent requests.
Three approaches were considered.

### Option A - Discretized time slots + unique constraint
Bookings are anchored to fixed-width slots (15 minutes). A unique index on
`(resource_id, slot_start)` lets the database reject a conflicting insert
outright - no explicit locking or retry logic required in application code.

Trade-off: users cannot book arbitrary ranges like 10:07-10:52; everything
aligns to the slot grid.

### Option B - Arbitrary ranges + serializable transaction
Bookings can start/end at any time. A `SERIALIZABLE` transaction checks for
overlap against existing bookings before inserting.

Trade-off: fully flexible scheduling, but higher lock contention and a real
risk of deadlocks/retries as booking volume grows on a popular resource.

### Option C - Arbitrary ranges + optimistic concurrency
Optimistic concurrency token per resource, retry on conflict.

Trade-off: works, but reasoning about retry behavior under real contention
(e.g. two users racing for the same freed-up slot) gets complicated to test
and to explain to a reviewer.

## Decision
Start with **Option A** for the MVP. It pushes the correctness guarantee down
into the database (a unique constraint is hard to get wrong), keeps the
application layer simple, and is easy to test deterministically (attempt two
inserts for the same slot in parallel, assert exactly one succeeds).

Option B is the natural next step if/when arbitrary-duration bookings become
a real product requirement, and is left documented here rather than
speculatively implemented now.

## Consequences
- `TimeSlot` (see `ResourceBooking.Domain.Entities.TimeSlot`) validates that a
  start time aligns to the slot grid.
- `BookingConfiguration` adds a unique index on `(ResourceId, SlotStart)`,
  filtered to exclude cancelled bookings so a freed-up slot can be rebooked
  without the old, cancelled row permanently occupying it.
- `BookingRepository.AddAsync` catches the resulting unique-index violation
  and rethrows it as `BookingConflictException`, so callers never see a raw
  `DbUpdateException`. Detection covers both SQL Server (production) and
  SQLite (test suite) - see `UniqueConstraintViolationDetector`.
- `BookingConcurrencyTests` (in `ResourceBooking.Infrastructure.Tests`) proves
  the guarantee directly: two concurrent `AddAsync` calls for the same
  resource/slot, exactly one succeeds.
- A booking that spans multiple slots (e.g. a 45-minute meeting) is modeled as
  multiple slot-bookings created within a single transaction - not yet
  implemented; `CreateBookingCommand` currently creates a single slot booking.
