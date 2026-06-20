# Hotel Stay Availability — Architecture & Implementation Plan

> **Challenge:** SkyRoute / Hotel Stay Availability  
> **Stack:** .NET 8 Minimal API · Angular 17+ · xUnit  
> **Status:** Pre-implementation — awaiting approval to generate code

---

## Phase 1: Challenge Analysis

### 1. Executive Summary

**Challenge Objective**  
Build a *Hotel Availability* feature for the fictional **SkyRoute** platform. The system aggregates room availability from two stub hotel providers, normalises the responses into a unified model, and supports a guest reservation flow with document validation. There is no authentication, no real external API, and no database persistence — the scope is a clean, extensible, offline-runnable full-stack application that demonstrates design thinking, code quality, and honest AI tooling usage.

**Business Requirements**
- Travellers can search available hotel rooms by destination, date range, and optional room type.
- Results from two providers (PremierStays, BudgetNests) are merged into a single normalised list.
- Travellers can reserve a room and receive a reference number.
- Document validation rules are enforced: international destinations require a Passport; domestic destinations accept a National ID.
- The system must be demonstrably extensible (add a third provider with zero changes to core logic).

**Technical Requirements**
- Backend: .NET 8+ Minimal API (C#)
- Frontend: Angular 17+ (Standalone Components)
- Tests: xUnit
- No real APIs, credentials, authentication, or persistent storage
- Must run fully offline from a clean `git clone`
- AI tooling (IDE-integrated) must be actively used and documented in `prompts.md`

**Functional Requirements**
1. Search endpoint queries both stubs, filters unavailable rooms, normalises, and returns a unified list.
2. BudgetNests rooms with `"available": false` are filtered out; PremierStays is always available.
3. Both providers return per-night rates; the API surfaces both per-night and total-stay price.
4. Room types (Standard, Deluxe, Suite) mapped to a unified enum.
5. Cancellation policies normalised: FreeCancellation / Flexible / NonRefundable.
6. Reservation endpoint validates document type against destination nationality class, returns 422 on mismatch.
7. Reservation lookup by reference number.
8. Frontend: search form → results list (sortable by total price) → reservation form → confirmation screen.
9. All four frontend states handled: results, empty, error, confirmation.

**Non-Functional Requirements**
- Offline-capable: no network calls to external services.
- Deterministic stubs: same inputs always produce same outputs (testable).
- Extensible provider interface (`IHotelProvider`) — Open/Closed Principle.
- Clean Code, SOLID, readable naming, XML docs on public APIs.
- 80%+ unit test coverage on business logic.
- No secrets or credentials in source control.

**Constraints**
- No real persistence — in-memory reservation store only.
- No authentication or authorisation.
- No real hotel APIs.
- Submission via public GitHub repository; `spec.md` committed before any implementation files.

**Acceptance Criteria**
1. `GET /hotels/search` returns normalised, merged results from both stubs.
2. `POST /hotels/reserve` returns a reference number; returns 422 on document mismatch.
3. `GET /hotels/reservation/{reference}` returns full reservation details.
4. All error states return correct HTTP status codes with structured error bodies.
5. Frontend shows all four states: results, empty, error, confirmation.
6. Results are sortable by total price.
7. Application runs end-to-end from `git clone` using only the README.
8. Unit tests are meaningful and cover business logic.
9. `spec.md` exists and was committed before implementation files.
10. `prompts.md` and `reflection.md` are present and substantive.

---

### 2. Requirement Analysis

| Requirement | Description | Priority | Source Document |
|---|---|---|---|
| R01 | `GET /hotels/search` endpoint with required params: destination, checkIn, checkOut | Must | API section |
| R02 | `roomType` query param is optional | Must | API section |
| R03 | Return 400 if destination, checkIn, or checkOut missing | Must | API section |
| R04 | Return 400 if checkOut not after checkIn | Must | API section |
| R05 | Query both stubs in parallel, merge results | Must | API section |
| R06 | Filter BudgetNests rooms where `available: false` | Must | Providers table |
| R07 | PremierStays: PascalCase JSON response, always available | Must | Providers table |
| R08 | BudgetNests: snake_case JSON response, may be unavailable | Must | Providers table |
| R09 | Normalise room types to unified enum: Standard, Deluxe, Suite | Must | Providers section |
| R10 | Normalise cancellation policy to: FreeCancellation, Flexible, NonRefundable | Must | Providers table |
| R11 | Expose per-night rate and total-stay price in response | Must | Providers section |
| R12 | PremierStays returns: rate, cancellation policy, amenities, star rating | Must | Providers table |
| R13 | BudgetNests returns: rate and policy only (no amenities, no star rating) | Must | Providers table |
| R14 | `POST /hotels/reserve` validates document, confirms reservation, returns reference | Must | API section |
| R15 | International destination → Passport required | Must | Document Validation section |
| R16 | Domestic destination → National ID accepted | Must | Document Validation section |
| R17 | At least 2 domestic cities defined | Must | Document Validation section |
| R18 | At least 3 international cities defined | Must | Document Validation section |
| R19 | Document validation both client-side and server-side | Must | Document Validation section |
| R20 | Return 422 with clear message on document mismatch | Must | Document Validation section |
| R21 | `GET /hotels/reservation/{reference}` returns reservation details | Must | API section |
| R22 | Use `IHotelProvider` interface with two DI-injected stub implementations | Must | API section |
| R23 | Stubs must be deterministic and cover representative scenarios | Must | API section |
| R24 | Frontend: search form (destination, check-in, check-out, room type) | Must | Frontend section |
| R25 | Frontend: results list with provider badge, room type, rates, cancellation policy | Must | Frontend section |
| R26 | Frontend: results sortable by total price | Must | Frontend section |
| R27 | Frontend: reservation form (guest name, document type, document number) | Must | Frontend section |
| R28 | Frontend: confirmation screen (reference number, provider, total price, policy) | Must | Frontend section |
| R29 | Frontend: all states — results, empty, error, confirmation | Must | Definition of Done |
| R30 | Design extensible for a third provider without reworking core flow | Must | Scope section |
| R31 | Must run fully offline from clean clone | Must | Scope section |
| R32 | `spec.md` committed before implementation files | Must | Definition of Done |
| R33 | `prompts.md` with AI prompts and decisions | Must | AI Tooling section |
| R34 | `reflection.md` with improvement notes | Must | Submission section |
| R35 | No secrets or credentials in repo | Must | Definition of Done |

---

### 3. Gap Analysis

| Requirement | Evidence Found | Missing Items | Risk |
|---|---|---|---|
| Provider stub data shape | High-level description (PascalCase, snake_case, fields listed) | Exact JSON schema not given — must be designed | Low — straightforward |
| City list (domestic/international) | "At least 2 domestic, 3 international" — no names given | Must define our own city list | Low — design choice |
| Cancellation policy values | PremierStays: FreeCancellation / NonRefundable; BudgetNests: Flexible / NonRefundable | Unified mapping must be designed | Low — clear enough |
| Reservation persistence | "No persistence" stated | Must use in-memory (ConcurrentDictionary) | Low — scope clear |
| Frontend framework | Angular listed first; any of 4 allowed | Angular chosen | None |
| Provider parallel query | "queries both stubs" implied | No explicit async requirement stated | Low — async/await natural |
| Total-stay price formula | "display both per-night and total-stay" | Formula: perNightRate × nights; nights = (checkOut − checkIn).Days | None |
| Error response schema | HTTP codes specified (400, 422) | Body shape not specified — RFC 7807 Problem Details chosen | Low |
| Guest data in reservation | "guest name, document type, document number" | No email or phone specified — excluded from scope | Low |
| Amenities format | PremierStays "amenities" | Array of strings assumed | Low |
| Star rating format | PremierStays "star rating" | Integer 1–5 assumed | Low |
| Reference number format | "returns reference number" | Format not specified — `"HS-" + GUID[..6]` chosen | Low |
| CORS configuration | Frontend calls backend | Must configure CORS for dev (localhost:4200) | Low |
| spec.md content | Must be committed first | Must define data models and interface contracts | Important — process |

---

### 4. Domain Analysis

**Entities**
- `Reservation` — aggregate root with identity (reference number). Carries all reservation data.
- `HotelRoom` — value object emitted by provider queries; no persistent identity.

**Value Objects**
- `SearchCriteria` — destination, checkIn, checkOut, optional roomType. Encapsulates validation.
- `CancellationPolicy` — enum: `FreeCancellation`, `Flexible`, `NonRefundable`.
- `RoomType` — enum: `Standard`, `Deluxe`, `Suite`.
- `DocumentType` — enum: `Passport`, `NationalId`.
- `DestinationClass` — enum: `Domestic`, `International`.

**Business Rules**
1. CheckOut must be strictly after CheckIn (minimum 1 night).
2. BudgetNests rooms with `available: false` must never appear in results.
3. Room type filter, when supplied, eliminates rooms of other types.
4. International destinations require DocumentType = Passport.
5. Domestic destinations accept DocumentType = NationalId or Passport.
6. Total price = perNightRate × (checkOut − checkIn).Days.
7. Reference number is unique and immutable once issued.
8. Stubs are deterministic — same input → same output set.

**Validation Rules**
- `destination`: required, non-empty, must match a known city.
- `checkIn` / `checkOut`: required, valid ISO dates. checkOut > checkIn.
- `roomType`: optional; if present, must be Standard / Deluxe / Suite (case-insensitive).
- `guestName`: required, non-empty.
- `documentType`: required, Passport or NationalId.
- `documentNumber`: required, non-empty.
- Document type must be valid for destination class → 422 on mismatch.

**Workflows**

*Search Workflow:*
```
Client → GET /hotels/search?...
→ Validate query params → 400 if invalid
→ Fan out to [PremierStaysProvider, BudgetNestsProvider] (parallel async)
→ Each provider maps own format to List<HotelRoom>
→ BudgetNests filters available:false internally
→ Apply optional roomType filter
→ Merge lists → Return unified JSON
```

*Reservation Workflow:*
```
Client → POST /hotels/reserve {...}
→ Validate request body → 400 on structural errors
→ Resolve destination class (domestic/international) → 400 if unknown city
→ Validate document type matches destination class → 422 on mismatch
→ Generate reference number (HS-XXXXXX)
→ Store in-memory
→ Return ReservationConfirmation
```

*Lookup Workflow:*
```
Client → GET /hotels/reservation/{reference}
→ Lookup in-memory store
→ 404 if not found → Return full ReservationDetail
```

---

## Phase 2: Architecture Design

### Architecture Principles

| Principle | Application |
|---|---|
| **Clean Architecture** | Domain → no dependencies on infrastructure. Application orchestrates via interfaces. Stubs are infrastructure — swappable. |
| **Open/Closed** | Adding a third provider = implement `IHotelProvider` + register in DI. Zero changes to `HotelSearchService`. |
| **Single Responsibility** | Each provider handles only its own mapping. Service only orchestrates. |
| **Dependency Inversion** | `HotelSearchService` depends on `IEnumerable<IHotelProvider>`, not concrete stubs. |
| **DRY** | Normalisation logic in shared mapper classes — not duplicated per provider. |
| **KISS** | No MediatR at 3 endpoints. Direct service injection is clearer. |
| **Secure by Design** | Input validation at boundary + domain layer. CORS locked. No secrets. |
| **DDD (selective)** | Value objects and domain rules in Domain layer — not leaked into endpoints. |

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Angular Frontend                          │
│  SearchFormComponent → ResultsListComponent                 │
│  ReservationFormComponent → ConfirmationComponent           │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP (JSON)
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              Presentation / API Layer (.NET 8)               │
│  Minimal API Endpoints (HotelEndpoints.cs)                  │
│  FluentValidation · Problem Details Middleware              │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Application Layer                           │
│  HotelSearchService — fans out to IEnumerable<IHotelProvider>│
│  ReservationService — validates, stores, retrieves          │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   Domain Layer                               │
│  Reservation entity · RoomType, CancellationPolicy enums   │
│  DocumentValidationService · CityClassificationService      │
│  Domain Exceptions (DocumentMismatchException, etc.)        │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│               Infrastructure / Stub Layer                    │
│  PremierStaysProvider : IHotelProvider (PascalCase stubs)   │
│  BudgetNestsProvider  : IHotelProvider (snake_case stubs)   │
│  InMemoryReservationStore : IReservationStore               │
└─────────────────────────────────────────────────────────────┘
```

### Project Structure

```
hotel-stay/
│
├── README.md
├── spec.md                          ← committed BEFORE any .cs or .ts files
├── prompts.md
├── reflection.md
│
├── HotelStay.Api/
│   ├── Program.cs
│   ├── Endpoints/HotelEndpoints.cs
│   ├── Validators/
│   │   ├── SearchQueryValidator.cs
│   │   └── ReserveRequestValidator.cs
│   └── Middleware/ExceptionMiddleware.cs
│
├── HotelStay.Application/
│   ├── Services/
│   │   ├── HotelSearchService.cs
│   │   └── ReservationService.cs
│   ├── Interfaces/
│   │   ├── IHotelProvider.cs
│   │   └── IReservationStore.cs
│   └── DTOs/
│       ├── SearchQueryDto.cs
│       ├── HotelRoomDto.cs
│       ├── ReserveRequestDto.cs
│       └── ReservationDetailDto.cs
│
├── HotelStay.Domain/
│   ├── Entities/Reservation.cs
│   ├── Enums/
│   │   ├── RoomType.cs
│   │   ├── CancellationPolicy.cs
│   │   ├── DocumentType.cs
│   │   └── DestinationClass.cs
│   ├── ValueObjects/SearchCriteria.cs
│   ├── Services/
│   │   ├── DocumentValidationService.cs
│   │   └── CityClassificationService.cs
│   └── Exceptions/
│       ├── DocumentMismatchException.cs
│       └── ReservationNotFoundException.cs
│
├── HotelStay.Infrastructure/
│   ├── Providers/
│   │   ├── PremierStaysProvider.cs
│   │   └── BudgetNestsProvider.cs
│   ├── Mappers/
│   │   ├── PremierStaysMapper.cs
│   │   └── BudgetNestsMapper.cs
│   └── Persistence/InMemoryReservationStore.cs
│
├── HotelStay.Tests/
│   ├── Unit/
│   │   ├── DocumentValidationServiceTests.cs
│   │   ├── CityClassificationServiceTests.cs
│   │   ├── HotelSearchServiceTests.cs
│   │   ├── ReservationServiceTests.cs
│   │   ├── PremierStaysMapperTests.cs
│   │   └── BudgetNestsMapperTests.cs
│   └── Integration/
│       ├── SearchEndpointTests.cs
│       └── ReservationEndpointTests.cs
│
└── hotel-stay-ui/                   ← Angular 17+ standalone SPA
    └── src/app/
        ├── features/
        │   ├── search/ (search-form, results-list)
        │   └── reservation/ (reservation-form, confirmation)
        ├── core/ (hotel.service.ts, models, interceptors)
        └── shared/ (provider-badge, loading-spinner, error-banner)
```

---

## Phase 3: API Design

### GET /hotels/search

**Query Params:** `destination` (required), `checkIn` (required, yyyy-MM-dd), `checkOut` (required, yyyy-MM-dd), `roomType` (optional)

**200 OK Response:**
```json
{
  "results": [
    {
      "provider": "PremierStays",
      "roomType": "Deluxe",
      "perNightRate": 149.00,
      "totalPrice": 447.00,
      "nights": 3,
      "cancellationPolicy": "FreeCancellation",
      "amenities": ["WiFi", "Breakfast", "Pool"],
      "starRating": 4
    },
    {
      "provider": "BudgetNests",
      "roomType": "Standard",
      "perNightRate": 72.00,
      "totalPrice": 216.00,
      "nights": 3,
      "cancellationPolicy": "NonRefundable",
      "amenities": null,
      "starRating": null
    }
  ],
  "destination": "Paris",
  "checkIn": "2025-08-01",
  "checkOut": "2025-08-04",
  "totalResults": 2
}
```

**400 Bad Request:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Bad Request",
  "status": 400,
  "detail": "checkOut must be after checkIn.",
  "instance": "/hotels/search"
}
```

---

### POST /hotels/reserve

**Request:**
```json
{
  "provider": "PremierStays",
  "roomType": "Deluxe",
  "destination": "Paris",
  "checkIn": "2025-08-01",
  "checkOut": "2025-08-04",
  "perNightRate": 149.00,
  "cancellationPolicy": "FreeCancellation",
  "guestName": "Jane Smith",
  "documentType": "Passport",
  "documentNumber": "AB123456"
}
```

**201 Created Response:**
```json
{
  "reference": "HS-A3F2C1",
  "provider": "PremierStays",
  "roomType": "Deluxe",
  "destination": "Paris",
  "checkIn": "2025-08-01",
  "checkOut": "2025-08-04",
  "nights": 3,
  "perNightRate": 149.00,
  "totalPrice": 447.00,
  "cancellationPolicy": "FreeCancellation",
  "guestName": "Jane Smith",
  "documentType": "Passport"
}
```

**422 Unprocessable Entity (document mismatch):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Document Validation Failed",
  "status": 422,
  "detail": "International destination 'Paris' requires a Passport. National ID is not accepted.",
  "instance": "/hotels/reserve"
}
```

---

### GET /hotels/reservation/{reference}

**200 OK:** Same shape as 201 response above.

**404 Not Found:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Not Found",
  "status": 404,
  "detail": "Reservation 'HS-XYZ999' was not found.",
  "instance": "/hotels/reservation/HS-XYZ999"
}
```

---

## Phase 4: Security Design

| Practice | Implementation |
|---|---|
| CORS | Restricted to `http://localhost:4200` |
| Secure Headers | `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY` |
| Input Validation | FluentValidation on all request models |
| Problem Details | RFC 7807 — no stack traces exposed |
| No Secrets | Stubs use in-memory data only |
| Logging | Serilog console sink — document numbers logged as `[REDACTED]` |
| Rate Limiting | .NET 8 built-in RateLimiter — 60 req/min per IP |
| Auth Placeholder | DI + middleware structure ready for JWT addition with zero endpoint changes |

---

## Phase 5: Database Design (In-Memory)

### City Classifications

| City | Class |
|---|---|
| Oslo | Domestic |
| Bergen | Domestic |
| Paris | International |
| London | International |
| Tokyo | International |

### Reservation Entity Fields

| Field | Type | Notes |
|---|---|---|
| Reference | string | PK — `"HS-" + Guid[..6].ToUpper()` |
| Provider | string | PremierStays / BudgetNests |
| RoomType | RoomType (enum) | Standard / Deluxe / Suite |
| Destination | string | Normalised city name |
| CheckIn | DateOnly | — |
| CheckOut | DateOnly | — |
| GuestName | string | — |
| DocumentType | DocumentType (enum) | Passport / NationalId |
| DocumentNumber | string | Never logged |
| PerNightRate | decimal | From provider |
| TotalPrice | decimal | PerNightRate × Nights |
| Nights | int | (CheckOut − CheckIn).Days |
| CancellationPolicy | CancellationPolicy (enum) | Normalised |
| CreatedAt | DateTimeOffset | Timestamp |

### Stub Data Design

**PremierStays** — always returns 3 rooms (Standard, Deluxe, Suite) per destination. Rates: Standard $99, Deluxe $149, Suite $229. Star ratings: 3, 4, 5. Amenities vary by room type.

**BudgetNests** — returns 4 entries: Standard (available), Deluxe (available), Suite (`available: false` — filtered), Standard-budget (available). Rates: $59, $89, $129, $45. This deterministically exercises the filter requirement.

---

## Phase 6: Backend Design (.NET 8)

### Framework: Minimal API (not Controllers)
Justified: exactly 3 routes, no complex routing, idiomatic .NET 8, spec explicitly names Minimal API.

### MediatR: Not Used
Justified: 3 use cases, simple linear orchestration — direct service injection is clearer (KISS).

### Key Interfaces

```csharp
public interface IHotelProvider
{
    string ProviderName { get; }
    Task<IReadOnlyList<HotelRoom>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default);
}

public interface IReservationStore
{
    void Save(Reservation reservation);
    Reservation? GetByReference(string reference);
}
```

### DI Registration (Program.cs)

```csharp
// Providers — all registered as IHotelProvider → injected as IEnumerable<IHotelProvider>
builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();

// Adding a third provider: ONE LINE — implement interface, add this line. Done.

builder.Services.AddSingleton<IReservationStore, InMemoryReservationStore>();
builder.Services.AddSingleton<CityClassificationService>();
builder.Services.AddSingleton<DocumentValidationService>();
builder.Services.AddScoped<HotelSearchService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddValidatorsFromAssemblyContaining<SearchQueryValidator>();
builder.Host.UseSerilog((ctx, cfg) => cfg.WriteTo.Console());
```

### Error Handling Map

| Exception | HTTP Status | Detail |
|---|---|---|
| `ValidationException` (FluentValidation) | 400 | Validation errors joined |
| `UnknownDestinationException` | 400 | "Unknown destination: '{city}'" |
| `DocumentMismatchException` | 422 | Specific human-readable message |
| `ReservationNotFoundException` | 404 | "Reservation '{ref}' not found" |
| `Exception` (catch-all) | 500 | "An unexpected error occurred" |

---

## Phase 7: Frontend Design (Angular 17+)

### Choice: Angular Standalone Components + Signals (not NgRx)
**Justification:** Linear user flow (search → results → reserve → confirm) — no cross-feature shared state. NgRx adds boilerplate without benefit. Signals (Angular 17) are idiomatic and sufficient. RxJS used only where natural (HttpClient observables).

### Key State Signals (SearchPageComponent)

```typescript
searchResults = signal<HotelRoomDto[]>([]);
selectedRoom = signal<HotelRoomDto | null>(null);
reservation = signal<ReservationDetail | null>(null);
isLoading = signal(false);
error = signal<string | null>(null);
view = signal<'search' | 'results' | 'reserve' | 'confirm'>('search');
```

### Client-Side Document Validation

```typescript
export const DOMESTIC_CITIES = ['Oslo', 'Bergen'];
export const INTERNATIONAL_CITIES = ['Paris', 'London', 'Tokyo'];

get allowedDocumentTypes(): DocumentType[] {
  return destinationClass === 'international'
    ? ['Passport']
    : ['Passport', 'NationalId'];
}
```

### All UX States

| State | Trigger |
|---|---|
| Loading | `isLoading() === true` |
| Results | `searchResults().length > 0` |
| Empty | `searchResults().length === 0 && !isLoading()` |
| Error | `error() !== null` |
| Confirmation | `reservation() !== null` |

---

## Phase 8: Testing Strategy

### Backend Unit Tests (xUnit)

| Test Class | Key Scenarios |
|---|---|
| `DocumentValidationServiceTests` | International+NationalId→throws; Domestic+Passport→passes; Domestic+NationalId→passes |
| `CityClassificationServiceTests` | Known domestic→Domestic; known international→International; unknown→throws |
| `HotelSearchServiceTests` | Both providers merge; BudgetNests unavailable filtered; roomType filter; parallel execution |
| `ReservationServiceTests` | Valid request→reference; doc mismatch→exception; unknown reference→exception |
| `PremierStaysMapperTests` | PascalCase JSON→HotelRoom; all room types; all policies |
| `BudgetNestsMapperTests` | snake_case JSON→HotelRoom; available:false filtered; policy mapping |

### Integration Tests (`WebApplicationFactory`)

| Test Class | Key Scenarios |
|---|---|
| `SearchEndpointTests` | 200 with results; 400 missing params; 400 bad dates; roomType filter |
| `ReservationEndpointTests` | 201 with reference; 422 doc mismatch; 400 missing fields; GET 200 and 404 |

### Coverage Targets

| Layer | Target |
|---|---|
| Domain services (business rules) | 100% |
| Application services | 90%+ |
| Infrastructure mappers | 90%+ |
| API endpoints (integration) | 80%+ scenario coverage |

---

## Phase 9: Documentation Plan

### Files to Produce

| File | Content |
|---|---|
| `spec.md` | Unified data models, interface contracts, all enums, city table, error schema, stub examples — **committed first** |
| `README.md` | Prerequisites, clone→run steps, test commands, architecture summary, assumptions |
| `prompts.md` | Verbatim AI prompts, judgement calls (Angular choice, no MediatR, Signals over NgRx, reference format, city selection) |
| `reflection.md` | What would improve with more time: real persistence, auth, Playwright E2E, pagination, currency handling, more cities, a11y audit |

---

## Phase 10: Implementation Sequence

Once architecture is approved, code will be generated in this order:

1. `spec.md` — data models and interface contracts (committed first, before any .cs or .ts)
2. Domain layer — enums, exceptions, `SearchCriteria`, `Reservation`, domain services
3. Application layer — interfaces, DTOs, `HotelSearchService`, `ReservationService`
4. Infrastructure layer — `PremierStaysProvider`, `BudgetNestsProvider`, `InMemoryReservationStore`, mappers
5. API layer — `Program.cs`, `HotelEndpoints.cs`, validators, middleware
6. Unit tests — domain and application
7. Integration tests — endpoint tests via `WebApplicationFactory`
8. Angular frontend — core services + models, search feature, reservation feature, shared components
9. `README.md`, `prompts.md`, `reflection.md`

---

*Architecture document prepared pre-implementation. No code generated yet. Awaiting approval.*
