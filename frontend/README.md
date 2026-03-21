# Resource Booking - Frontend

React + TypeScript client for the Resource Booking API.

## Structure

```
src/
  api/          fetch wrapper, ProblemDetails-aware error handling, typed endpoint calls
  auth/         AuthContext (login/register/logout, token persistence), ProtectedRoute
  components/   shared UI (Layout/nav, AvailabilityGrid)
  pages/        one component per route
```

## Routes

| Path              | Access       | Page               |
|--------------------|--------------|---------------------|
| `/login`           | public       | LoginPage           |
| `/register`        | public       | RegisterPage        |
| `/`                | any user     | ResourcesPage - browse resources, pick a date, book a slot |
| `/my-bookings`     | any user     | MyBookingsPage - list own bookings, cancel |
| `/admin/resources` | Admin only   | AdminResourcesPage - create/deactivate resources |

`ProtectedRoute` redirects unauthenticated visitors to `/login`, and redirects
non-Admin users away from `/admin/resources` to `/`.

## Local development

```bash
npm install
npm run dev
```

The dev server proxies `/api/*` to `https://localhost:5001` (see
`vite.config.ts`) so the backend must be running for API calls to succeed -
see the root README for how to start it. There's no CORS setup because of
this proxy; production deployment (a different origin) is a separate,
not-yet-done milestone.

## Auth

The JWT returned by `/api/auth/login` and `/api/auth/register` is stored in
`localStorage` and decoded client-side (in `AuthContext`) purely to restore
`{ userId, email, role }` after a page reload - the backend re-validates the
token's signature on every request regardless, so nothing security-relevant
depends on that decode.

## Testing

Not set up yet. Backend correctness (including everything these API calls
depend on - auth, validation, conflict handling) is covered by the three
xunit test projects in `../tests`.
