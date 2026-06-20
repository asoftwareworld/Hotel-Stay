# reflection.md — What I Would Improve With More Time

This file reflects honestly on the trade-offs made in the current implementation and what a production-grade version would look like.

---

## What Works Well

- **Extensibility is real.** Adding a third hotel provider genuinely requires only implementing `IHotelProvider` and one DI registration line. The architecture earns this claim.
- **Domain isolation is clean.** The Domain layer has zero dependencies on infrastructure or HTTP. Business rules (document validation, city classification) are pure C# and unit-testable in isolation.
- **Deterministic stubs make tests meaningful.** The BudgetNests stub intentionally includes a `Suite` with `available: false`. Tests can assert on exact room counts, not just "not empty".
- **RFC 7807 Problem Details.** Structured error responses make the 422 document mismatch message actionable for both UI and API consumers.

---

## What I Would Improve With More Time

### 1. Persistence
Replace `InMemoryReservationStore` with a real persistence layer. **SQLite + EF Core** would be the first step — zero infrastructure cost, single file, still runs offline. This would enable:
- Reservations surviving a process restart
- Proper `async` store operations (currently sync)
- Migration history

### 2. End-to-End Tests
Add **Playwright** E2E tests covering the full browser flow: search → select room → fill reservation form → confirm. Currently only unit and integration tests exist. E2E tests would catch UI regressions (e.g. Angular form validation not wiring correctly) that integration tests miss.

### 3. More Cities and Dynamic Classification
The city list is currently hardcoded in a static dictionary. A production system would load this from configuration or a lookup table, making it easy to add cities without code changes. A configuration-driven approach would also allow the Angular frontend to fetch the city list from the API rather than mirroring a hardcoded constant.

### 4. Pagination and Search Refinement
The search currently returns all matching results. With real providers returning hundreds of rooms, pagination (`?page=1&pageSize=20`) and additional filters (max price, amenities, star rating) would be essential. The `SearchQueryDto` and endpoint are designed to accommodate this without breaking changes.

### 5. Currency Handling
Prices are implicitly USD. A production version would include a `currency` field in `HotelRoomDto`, support multi-currency providers, and display prices using the user's locale. The `Money` value object concept is alluded to in the domain analysis but not implemented.

### 6. Authentication and Authorisation
The spec explicitly excludes auth, but the architecture is auth-ready. `Program.cs` can add `AddAuthentication().AddJwtBearer()` without touching any endpoint logic — the DI and middleware pipeline are structured for it. A real system would require users to log in before reserving, and reservation lookup would be scoped to the authenticated user.

### 7. Error Recovery in Search
If one provider fails (network error in a real integration), `Task.WhenAll` causes the entire search to fail. A more resilient design would catch per-provider exceptions, log them, and return partial results with a warning in the response. `Task.WhenAll` with individual try/catch per task, or the `Polly` library for retry/circuit-breaker, would handle this.

### 8. Accessibility Audit
The Angular UI uses semantic HTML and ARIA patterns, but a formal WCAG 2.1 AA audit would be needed before shipping. Specific areas: colour contrast on cancellation policy badges (amber on white may fail AA), focus management when navigating between search → reserve → confirm views, and screen reader announcements for dynamic result updates.

### 9. Frontend Unit Test Coverage
Only the `HotelService` has unit tests in the current implementation. With more time, I would add component tests for:
- `SearchFormComponent` — cross-field date validation
- `ResultsListComponent` — sort order toggle
- `ReservationFormComponent` — document type filtering for domestic vs international

### 10. OpenAPI / Swagger Documentation
Adding `Microsoft.AspNetCore.OpenApi` and Scalar/Swashbuckle to the API would provide interactive documentation that runs alongside the backend. The endpoint definitions in `HotelEndpoints.cs` are structured to support `.WithName()` and `.WithSummary()` annotations with minimal effort.

### 11. Docker Compose
A `docker-compose.yml` running both the API and the Angular SPA (pre-built) would make the "clone and run" experience even smoother — no SDK or Node prerequisite needed, just Docker.

---

## Honest AI Reflection

AI tooling (Claude in Cowork mode) was used throughout, from analysis to code generation. The value was highest in:
- **Requirement extraction and gap analysis** — turning a 3-page PDF into 35 explicit, numbered requirements with risk ratings
- **Architecture documentation** — generating the architecture diagram, project structure, and API contract examples before any code was written
- **Boilerplate generation** — .csproj files, DI registration, test scaffolding

The areas requiring more human judgement:
- **Design decisions** — MediatR vs direct, NgRx vs Signals, city selection — these were explicitly reasoned *before* instructing the AI, not delegated to it
- **Edge case handling** — the `ReservationFormComponent` needing a separate `destination` input (because `HotelRoom` doesn't carry search context) was identified during design review
- **Test quality** — specifying *which scenarios* to test (not just "write tests") was a deliberate human decision to ensure tests are meaningful rather than cosmetic

The AI is most valuable when given precise, well-reasoned instructions. Vague prompts produce vague code. The time invested in the architecture plan (Phase 1–9) paid off in the quality of generated implementation.
