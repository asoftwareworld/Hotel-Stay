# HotelStay — SkyRoute Hotel Availability

A full-stack hotel availability feature built for the SkyRoute platform. Travellers search for rooms across two stub providers, view normalised results, and complete a reservation with document validation.

---

## Architecture Overview

```
hotel-stay/
├── HotelStay.Api/          .NET 8 Minimal API — HTTP boundary, routing, validation
├── HotelStay.Application/  Orchestration — services, interfaces, DTOs
├── HotelStay.Domain/       Business logic — entities, rules, exceptions
├── HotelStay.Infrastructure/ Stubs (PremierStays, BudgetNests) + in-memory store
├── HotelStay.Tests/        xUnit — unit + integration tests
└── hotel-stay-ui/          Angular 17 standalone SPA
```

**Key design decisions:**
- `IHotelProvider` interface — adding a third provider = one class + one DI line, zero core changes
- Clean Architecture — Domain has no dependencies on infrastructure
- In-memory `ConcurrentDictionary` for reservation storage (no database required)
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

Backend listens on **http://localhost:5000** and **https://localhost:5001**.

### 3. Start the frontend (new terminal)

```bash
cd hotel-stay-ui
npm install
npm start
```

Frontend available at **http://localhost:4200**. The Angular proxy forwards `/hotels/*` requests to the backend automatically — no manual CORS configuration needed during development.

---

## Running Tests

### Backend (xUnit)

```bash
dotnet test
```

Run with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend (Jasmine / Karma)

```bash
cd hotel-stay-ui
npm test
```

---

## API Endpoints

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

---

## Stub Provider Behaviour

**PremierStays** — always returns 3 rooms (Standard, Deluxe, Suite) with full detail (amenities, star rating). Always available.

**BudgetNests** — returns 4 entries. The Suite entry has `available: false` and is filtered out, leaving 3 results. Minimal detail (rate and policy only).

---

## Document Validation

- International destination + National ID → **HTTP 422** with message:  
  `"International destination 'Paris' requires a Passport. National ID is not accepted."`
- Validated both client-side (Angular form disables invalid options) and server-side.

---

## Assumptions

1. Currency is USD throughout (not specified in brief).
2. Prices are static per room type — providers don't vary by date (deterministic stubs).
3. A Passport is always accepted at domestic destinations (superset of National ID).
4. Unknown destination cities return HTTP 400 (not silently ignored).
5. Reservation data persists only for the lifetime of the backend process (in-memory by design).
6. No minimum advance booking requirement — any future dates accepted.
7. Reference numbers are globally unique per process run (GUID-based).

---

## No Credentials Required

This application runs fully offline. There are no API keys, connection strings, or external service calls.
