# prompts.md — AI Tooling Log

This file documents significant AI prompts used during this challenge, along with the reasoning and judgement calls made at each stage.

---

## Phase 1 — Challenge Analysis

**Prompt (analysis):**
> "Review the PDF challenge document and produce a complete architecture and implementation plan before generating any code. Include: executive summary, requirement table, gap analysis, domain analysis, architecture diagram, project structure, API design with JSON examples, security design, database design, backend design, frontend design, testing strategy, and documentation plan."

**Judgement calls made during analysis:**
- Identified that the brief says "at least 2 domestic, 3 international" cities but names none — chose Oslo, Bergen (domestic) and Paris, London, Tokyo (international) as representative, recognisable cities.
- Identified that the brief does not specify error response body shape — chose RFC 7807 Problem Details as the industry standard.
- Identified that "display both per-night and total-stay price" implies a formula: `totalPrice = perNightRate × nights` — documented in spec.md.
- Identified that BudgetNests Suite with `available: false` needed to be *deterministic* in the stub to make tests meaningful — designed the stub to always include this entry.

---

## Phase 2 — Architecture Design

**Prompt:**
> "Design Clean Architecture for this .NET 8 Minimal API. Justify: no MediatR, no AutoMapper, Minimal API over Controllers, Signals over NgRx, xUnit. Design IHotelProvider so a third provider can be added with zero changes to HotelSearchService."

**Key judgement calls:**

**Why Minimal API, not Controllers?**
The brief explicitly names "Minimal API". With exactly 3 endpoints, controller ceremony (class, attributes, ActionResult<T> return types) adds no value. Minimal API is more idiomatic for .NET 8 and keeps `HotelEndpoints.cs` focused.

**Why no MediatR?**
MediatR is excellent for large command/query surfaces. At 3 use cases (Search, Reserve, GetReservation), direct service injection is cleaner and easier to follow in a code review. Adding MediatR for 3 handlers would be over-engineering (KISS principle).

**Why no AutoMapper?**
The mapping surface is small and explicit. Explicit mapping methods in mapper classes are more readable and debuggable than configured AutoMapper profiles. No magic — every property assignment is visible.

**Why IEnumerable<IHotelProvider> for extensibility?**
Registering multiple implementations of the same interface and injecting `IEnumerable<IHotelProvider>` is idiomatic .NET DI. `HotelSearchService` never knows how many providers exist — it fans out to all of them with `Task.WhenAll`. Adding a third provider is:
1. Implement `IHotelProvider`
2. Add `services.AddSingleton<IHotelProvider, ThirdProvider>()` to `InfrastructureServiceExtensions.cs`
3. Done. Zero other changes.

**Why Angular Signals over NgRx?**
The user flow is strictly linear: search → results → reserve → confirm. There is no shared state between unrelated feature trees, no time-travel debugging need, no complex async side-effect chains. NgRx would add 3–4 boilerplate files (actions, reducer, selectors, effects) per feature for no architectural benefit. Angular 17 Signals handle this cleanly with far less ceremony. RxJS is still used where it naturally fits (HttpClient observables).

---

## Phase 3 — spec.md

**Prompt:**
> "Write spec.md containing all data models, interface contracts, enums, city classification table, error response schema, and stub data examples. This must be committed before any implementation files."

**Judgement call:** The reference number format `"HS-" + Guid.NewGuid().ToString("N")[..6].ToUpper()` was chosen because: (a) the "HS-" prefix makes it domain-identifiable, (b) 6 hex chars gives 16^6 = ~16 million combinations — sufficient for a demo, (c) it's short enough to display clearly in the UI.

---

## Phase 4 — Backend Code Generation

**Prompt (backend agent):**
> Full specification of all 43 backend files with exact class names, method signatures, DI registrations, and implementation rules. Key constraints: no TODOs, async/await throughout, FluentAssertions in tests, WebApplicationFactory for integration tests, BudgetNests must filter available:false before mapping.

**Judgement calls:**

- `DocumentValidationService.Validate` signature includes `destinationCity` (string) so the error message can include the city name for a helpful 422 response. This is more user-friendly than a generic message.
- `ExceptionMiddleware` implements `IMiddleware` rather than inline lambda middleware — this enables constructor injection of `ILogger` cleanly.
- `CityClassificationService` uses `StringComparer.OrdinalIgnoreCase` — the brief doesn't specify case sensitivity, but a case-insensitive lookup is more forgiving for real-world use.
- Rate limiter is added at the middleware level to demonstrate security-conscious design even though the spec doesn't require it.
- `public partial class Program {}` enables `WebApplicationFactory<Program>` in integration tests without a separate test host assembly.

---

## Phase 5 — Frontend Code Generation

**Prompt (frontend agent):**
> Full specification of all Angular files with: standalone components, OnPush change detection, Angular 17 @if/@for control flow, signal-based state in SearchPageComponent, client-side document validation using shared city-classifications constants, ReservationFormComponent taking both `room` and `destination` inputs.

**Judgement calls:**

- `ReservationFormComponent` receives `destination` as a separate input because `HotelRoom` doesn't carry destination — it's a per-search parameter. The `SearchPageComponent` stores `lastDestination` in a signal and passes it down. This keeps the model clean (a room offer shouldn't carry search context).
- `ResultsListComponent` uses a computed signal for sorted rooms — this means sorting is reactive and zero-cost when not changed, rather than re-sorting on every change detection cycle.
- The `error.interceptor.ts` extracts `error.error?.detail` (the RFC 7807 Problem Details `detail` field) so user-facing error messages are the backend's human-readable strings, not raw HTTP messages.
- Angular proxy (`proxy.conf.json`) is used instead of a CORS wildcard — this is safer for development and means the production CORS config remains restrictive.

---

## Phase 6 — Documentation

**Prompt:**
> "Write README.md with setup, run steps, assumptions, and architecture summary. Write prompts.md capturing all AI prompts and judgement calls. Write reflection.md on improvements."

---

## Summary of AI Usage Pattern

AI was used across the full SDLC:
- **Analysis phase:** extracting and tabulating requirements, identifying gaps
- **Architecture phase:** generating and evaluating architectural options
- **Design phase:** designing API contracts, database schema, component hierarchy
- **Implementation phase:** generating all C# and TypeScript code
- **Testing phase:** specifying test scenarios and generating test classes
- **Documentation phase:** generating README, spec.md, this file

The AI did not replace judgement — every design decision (MediatR vs direct, NgRx vs Signals, reference number format, city selection) was explicitly reasoned before instructing the AI to implement it.
