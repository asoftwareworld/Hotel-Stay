# HotelStay — Specification (Data Models & Interface Contracts)

> **Committed before any implementation files.**  
> This document is the contract between all layers of the system.

---

## 1. Enumerations

### RoomType
```
Standard
Deluxe
Suite
```

### CancellationPolicy
```
FreeCancellation   – cancel up to 48 h before check-in (PremierStays)
Flexible           – cancel up to 24 h before check-in (BudgetNests)
NonRefundable      – no cancellation (both providers)
```

### DocumentType
```
Passport
NationalId
```

### DestinationClass
```
Domestic
International
```

---

## 2. City Classifications

| City   | Class         |
|--------|---------------|
| Oslo   | Domestic      |
| Bergen | Domestic      |
| Paris  | International |
| London | International |
| Tokyo  | International |

**Rule:** International destination → Passport required. Domestic destination → NationalId or Passport accepted.

---

## 3. Data Models

### HotelRoom (unified, normalised)
| Field              | Type                 | Nullable | Notes                                  |
|--------------------|----------------------|----------|----------------------------------------|
| Provider           | string               | No       | "PremierStays" or "BudgetNests"        |
| RoomType           | RoomType             | No       | Unified enum                           |
| PerNightRate       | decimal              | No       | Provider's rate per night              |
| TotalPrice         | decimal              | No       | PerNightRate × Nights                  |
| Nights             | int                  | No       | (CheckOut − CheckIn).Days              |
| CancellationPolicy | CancellationPolicy   | No       | Unified enum                           |
| Amenities          | string[]             | Yes      | PremierStays only; null for BudgetNests|
| StarRating         | int?                 | Yes      | PremierStays only (1–5); null for BudgetNests |

### Reservation (in-memory entity)
| Field              | Type               | Notes                                     |
|--------------------|--------------------|-------------------------------------------|
| Reference          | string             | PK — "HS-" + Guid[..6].ToUpper()         |
| Provider           | string             |                                           |
| RoomType           | RoomType           |                                           |
| Destination        | string             | Normalised city name                      |
| CheckIn            | DateOnly           |                                           |
| CheckOut           | DateOnly           |                                           |
| GuestName          | string             |                                           |
| DocumentType       | DocumentType       |                                           |
| DocumentNumber     | string             | Never logged                              |
| PerNightRate       | decimal            |                                           |
| TotalPrice         | decimal            | PerNightRate × Nights                     |
| Nights             | int                |                                           |
| CancellationPolicy | CancellationPolicy |                                           |
| CreatedAt          | DateTimeOffset     | UTC timestamp of creation                 |

---

## 4. Interface Contracts

### IHotelProvider
```csharp
public interface IHotelProvider
{
    /// <summary>Human-readable provider name included in results.</summary>
    string ProviderName { get; }

    /// <summary>
    /// Returns available rooms for the given search criteria.
    /// Implementations are responsible for filtering unavailable rooms internally.
    /// </summary>
    Task<IReadOnlyList<HotelRoom>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default);
}
```

**Contract rules:**
- Must never throw for a normal "no results" case — return empty list.
- Must filter unavailable rooms before returning.
- Must map provider-specific room types and policies to unified enums.
- Must be deterministic: same `SearchCriteria` → same result set.

### IReservationStore
```csharp
public interface IReservationStore
{
    /// <summary>Persists a reservation. Throws if reference already exists.</summary>
    void Save(Reservation reservation);

    /// <summary>Returns the reservation or null if not found.</summary>
    Reservation? GetByReference(string reference);
}
```

---

## 5. API Request / Response Schemas

### GET /hotels/search

**Query parameters:**
```
destination  string   required
checkIn      string   required   yyyy-MM-dd
checkOut     string   required   yyyy-MM-dd
roomType     string   optional   Standard | Deluxe | Suite
```

**200 Response:**
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

### POST /hotels/reserve

**Request body:**
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

**201 Response:**
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

### GET /hotels/reservation/{reference}

**200 Response:** Same shape as the 201 response above.

---

## 6. Error Response Schema (RFC 7807 Problem Details)

All errors return this structure:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "string",
  "status": 400,
  "detail": "Human-readable explanation.",
  "instance": "/hotels/search"
}
```

| Status | Trigger |
|--------|---------|
| 400    | Missing required params; checkOut not after checkIn; unknown destination city |
| 404    | Reservation reference not found |
| 422    | Document type invalid for destination (International + NationalId) |
| 500    | Unhandled exception (no stack trace exposed) |

---

## 7. Provider Stub Data

### PremierStays — Raw Response Shape (PascalCase JSON)
```json
[
  {
    "RoomType": "Standard",
    "RatePerNight": 99.00,
    "CancellationPolicy": "FreeCancellation",
    "Amenities": ["WiFi", "TV"],
    "StarRating": 3
  },
  {
    "RoomType": "Deluxe",
    "RatePerNight": 149.00,
    "CancellationPolicy": "FreeCancellation",
    "Amenities": ["WiFi", "Breakfast", "Pool"],
    "StarRating": 4
  },
  {
    "RoomType": "Suite",
    "RatePerNight": 229.00,
    "CancellationPolicy": "NonRefundable",
    "Amenities": ["WiFi", "Breakfast", "Pool", "Spa", "Concierge"],
    "StarRating": 5
  }
]
```

### BudgetNests — Raw Response Shape (snake_case JSON)
```json
[
  {
    "room_type": "standard",
    "rate_per_night": 59.00,
    "cancellation_policy": "non_refundable",
    "available": true
  },
  {
    "room_type": "deluxe",
    "rate_per_night": 89.00,
    "cancellation_policy": "flexible",
    "available": true
  },
  {
    "room_type": "suite",
    "rate_per_night": 129.00,
    "cancellation_policy": "flexible",
    "available": false
  },
  {
    "room_type": "standard",
    "rate_per_night": 45.00,
    "cancellation_policy": "non_refundable",
    "available": true
  }
]
```

Note: The Suite entry with `available: false` is intentional — it verifies the filtering logic in tests.

---

## 8. Cancellation Policy Mapping

| Provider Value       | Unified Enum      |
|----------------------|-------------------|
| `FreeCancellation`   | FreeCancellation  |
| `NonRefundable`      | NonRefundable     |
| `flexible`           | Flexible          |
| `non_refundable`     | NonRefundable     |

---

## 9. Reference Number Format

`"HS-" + Guid.NewGuid().ToString("N")[..6].ToUpper()`

Example: `HS-A3F2C1`
