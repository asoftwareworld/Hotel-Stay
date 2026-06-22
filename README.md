# HotelStay — SkyRoute Hotel Availability

A full-stack hotel availability feature built for the SkyRoute platform. Travellers register, log in, search for rooms across two stub providers, view normalised results, and complete a reservation with document validation.

---

## Architecture Overview

```
hotel-stay/
├── HotelStay.Api/            .NET 8 Web API (controllers) — HTTP boundary, routing, auth middleware
├── HotelStay.Application/    Orchestration — services, interfaces, DTOs
├── HotelStay.Domain/         Business logic — entities, rules, exceptions
├── HotelStay.Infrastructure/ Stubs (PremierStays, BudgetNests) + in-memory stores
├── HotelStay.Tests/          xUnit — 56 unit + integration tests
└── hotel-stay-ui/            Angular 17 standalone SPA
```

**Key design decisions:**
- `IHotelProvider` interface — adding a third provider = one class + one DI line, zero core changes
- Clean Architecture — Domain has no dependencies on infrastructure
- In-memory `ConcurrentDictionary` for reservation and user/token storage (no database required)
- JWT access tokens (15 min) + rotating refresh tokens (7 days) — stateless API access with secure session renewal
- MVC controllers (`[ApiController]`) for all HTTP routing — `AuthController` and `HotelsController`
- Angular Signals (not NgRx) — linear flow, no cross-feature state
- RFC 7807 Problem Details for all error responses

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| Node.js | 20+ |
| Angular CLI | 17+ (`npm install -g @angular/cli`) |

---

## Running Locally

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd hotel-stay
```

### 2. Start the backend

```bash
cd HotelStay.Api
dotnet run
```

Backend listens on **http://localhost:5000**.

### 3. Start the frontend (new terminal)

```bash
cd hotel-stay-ui
npm install
npm start
```

Frontend available at **http://localhost:4200**. On first load you are redirected to `/register` or `/login`.

---

## Running Tests

### Backend (xUnit)

```bash
dotnet test
```

56 tests — unit (AuthService, ReservationService, providers, domain services) + integration (auth, search, reservation endpoints).

Run with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## Default User

A seeded admin account is available immediately on startup — no registration needed for local development.

| Field | Value |
|-------|-------|
| Email | `admin@hotelstay.com` |
| Username | `admin` |
| Password | `Admin1234` |
| Role | Admin |

Use these credentials with `POST /auth/login` to get a token pair and start making requests.

---

## Authentication

All hotel API endpoints require a valid JWT. Obtain one by registering or logging in.

### Endpoints

| Method | Route | Auth required | Description |
|--------|-------|:---:|-------------|
| POST | `/auth/register` | — | Create account, returns token pair |
| POST | `/auth/login` | — | Log in, returns token pair |
| POST | `/auth/refresh` | — | Exchange refresh token for new token pair |
| POST | `/auth/logout` | ✓ | Revoke refresh token |

### Token pair response

```json
{
  "accessToken":  "<JWT — valid 15 minutes>",
  "refreshToken": "<opaque — valid 7 days, rotated on each use>",
  "expiresIn":    900
}
```

### Registration rules

- Email must be a valid address
- Username is required and must be at least 2 characters
- Password must be at least 8 characters

### Using the access token

```http
GET /hotels/search?destination=Paris&checkIn=2025-08-01&checkOut=2025-08-04
Authorization: Bearer <accessToken>
```

### Token rotation

Every call to `/auth/refresh` consumes the submitted refresh token and issues a fresh pair. Reusing a consumed token returns **401**. The Angular frontend handles refresh automatically — on a 401 response it calls `/auth/refresh`, retries the original request, and redirects to `/login` only if the refresh itself fails.

### Error codes

| Status | Meaning |
|--------|---------|
| 401 | Missing/expired token, wrong password, or invalid refresh token |
| 409 | Email already registered |

---

## Hotel API Endpoints

All routes require `Authorization: Bearer <accessToken>`.

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/hotels/search?destination=Paris&checkIn=2025-08-01&checkOut=2025-08-04` | Search available rooms |
| POST | `/hotels/reserve` | Reserve a room |
| GET | `/hotels/reservation/{reference}` | Retrieve reservation by reference |

**roomType** query param is optional: `Standard`, `Deluxe`, or `Suite`.

### Example search response

```json
{
  "results": [
    {
      "provider": "BudgetNests",
      "roomType": "Standard",
      "perNightRate": 45.00,
      "totalPrice": 135.00,
      "nights": 3,
      "cancellationPolicy": "NonRefundable",
      "amenities": null,
      "starRating": null
    },
    {
      "provider": "PremierStays",
      "roomType": "Standard",
      "perNightRate": 99.00,
      "totalPrice": 297.00,
      "nights": 3,
      "cancellationPolicy": "FreeCancellation",
      "amenities": ["WiFi", "TV"],
      "starRating": 3
    }
  ],
  "destination": "Paris",
  "checkIn": "2025-08-01",
  "checkOut": "2025-08-04",
  "totalResults": 2
}
```

---

## Supported Destinations

| City | Type | Valid Documents |
|------|------|----------------|
| Oslo | Domestic | Passport, National ID |
| Bergen | Domestic | Passport, National ID |
| Paris | International | Passport only |
| London | International | Passport only |
| Tokyo | International | Passport only |

Unknown destinations return **HTTP 400**.

---

## Stub Provider Behaviour

**PremierStays** — always returns 3 rooms (Standard, Deluxe, Suite) with full detail (amenities, star rating).

**BudgetNests** — returns 4 entries. The Suite entry has `available: false` and is filtered out, leaving 3 results. Minimal detail (rate and policy only).

---

## Document Validation

- International destination + National ID → **HTTP 422**:  
  `"International destination 'Paris' requires a Passport. National ID is not accepted."`
- Validated both client-side (Angular form disables invalid options) and server-side.

---

## Frontend Flow

```
/ → /search (auth guard)
      ↓ not authenticated
    /login  ←→  /register
      ↓ success
    /search
      nav bar → Sign out → /login
```

The Angular `authInterceptor` attaches `Authorization: Bearer ...` to every outgoing request. On a 401 it silently refreshes the token and retries — only a failed refresh redirects the user to `/login`.

---

## Assumptions

1. Currency is USD throughout.
2. Prices are static per room type — providers don't vary by date (deterministic stubs).
3. A Passport is always accepted at domestic destinations.
4. Reservation and user data persist only for the lifetime of the backend process (in-memory by design).
5. No minimum advance booking requirement — any future dates accepted.
6. Reference numbers are globally unique per process run (GUID-based).
7. JWT signing key is read from `appsettings.json`. In production this must move to an environment variable or secrets manager.
