# Point of Sale (POS)

A **Point-of-Sale** system built around **.NET 10**, made up of a desktop
**WPF** application, a shared **ASP.NET Core Web API** (`POS.Api`), and an
**Expo / React Native** mobile companion app (`Mobile/`) for managers. All
three sit on the same **Business Logic** and **Data-Access** layers, following
**Clean Architecture**, the **MVVM** pattern (WPF), and an **EF Core + ADO.NET**
hybrid data-access strategy.

---

## Applications

### WPF (desktop)

The primary, full-featured Point-of-Sale application, used for day-to-day
cashier and manager operations. Connects directly to SQL Server through the
existing BLL/DAL.

- **Sales / Checkout** — cashier dashboard with cart, quick actions, modifiers,
  and both sale and recent-sale flows.
- **Transactions** — create, browse, search, and manage purchase receipts with
  notes; void and payment handling.
- **Products, Categories, Sizes, Modifiers** — full CRUD management with
  multi-language business data translations (Product / Category / Size /
  Modifier translations).
- **Shifts** — open / close day-shift management with shift summaries.
- **Reports & KPIs** — reporting, top-product, recent-sale, and KPI queries.
- **Users, Roles & Sessions** — role-based access control with BCrypt password
  hashing, login / "login as" flows, and audit logging.
- **Receipt Printing** — thermal-receipt printing via QuestPDF-style PDF
  generation (see `Services/PrintingService.cs`).
- **Localization** — UI strings in **English, Arabic, and Malayalam**
  (`WPF/Localization/en.xaml`, `ar.xaml`, `ml.xaml`) with runtime
  language switching.

### Mobile (manager companion)

A read-focused **Expo / React Native / TypeScript** app for managers, living
at `Mobile/` as a sibling of `WPF/` and `POS.Api/`. It talks to `POS.Api`
only — never directly to SQL Server.

- **Dashboard** — KPI overview and recent transactions.
- **Transactions** — paginated, period-filterable list plus receipt-number
  search, and a full transaction detail view.
- **Products & Categories** — category-tabbed product browsing with search,
  product detail, and a read-only nested category browser.
- **Shifts** — shift list and detail (reconciliation summary, transactions).
- **Reports** — sales summary KPIs, a sales-by-day chart, and a top-categories
  breakdown with drill-down.
- **Auth** — token-based login with remember-me and optional biometric unlock.
- **Settings** — biometric toggle, connection info, logout.

Currently online-only; offline read caching is planned but not yet built.

### API (`POS.Api`)

An ASP.NET Core Web API that exposes the WPF app's existing BLL/DAL to the
mobile client as DTO-based REST endpoints — it doesn't duplicate business
logic. See `docs/api-contract.md` for the current endpoint list and DTO
conventions.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Desktop runtime / UI | .NET 10 (`net10.0-windows`), WPF |
| Desktop UI pattern | MVVM (CommunityToolkit-based ViewModels, `RelayCommand`) |
| Mobile runtime | React Native + TypeScript + Expo |
| Mobile server state | TanStack Query |
| Mobile navigation | React Navigation |
| ORM | EF Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Low-level SQL | ADO.NET for bulk / reporting / performance-critical paths |
| Database | SQL Server (see `DAL/db/` for schema definition) |
| Business layer | Custom services + `Microsoft.Extensions.DependencyInjection` |
| API | ASP.NET Core Web API (`POS.Api`) |
| PDF / Printing | QuestPDF, ClosedXML (Excel export) |
| Password hashing | BCrypt.Net-Next |
| Testing | xUnit, FluentAssertions |

---

## Architecture

```
WPF (Views + ViewModels)          Mobile (React Native)
        │                                  │  HTTPS / REST / JSON
        │ (interfaces only,                ▼
        │  e.g. IProductService)   POS.Api (ASP.NET Core)
        ▼                                  │
Application / BLL (use cases / services — defines intent)
        │  (calls repository / query interfaces)
        ▼
Infrastructure / DAL (decides EF Core vs ADO.NET, implements repositories)
        │
        ▼
Data (SQL Server — DbContext / SqlConnection)
```

The mobile app never talks to SQL Server, ADO.NET, or the DAL directly — all
mobile access goes through `POS.Api`. See `docs/architecture.md` for the full
breakdown.

### Project layout

| Project / folder | Role |
|---------|------|
| `Contracts/` | Shared contracts: DTOs, enums, service/repository interfaces |
| `DAL/` | Data-access layer: EF Core `DbContext`, repositories, entities, raw-SQL schema |
| `BLL/` | Business-logic layer: services implementing the application use cases |
| `WPF/` | WPF UI: Views, ViewModels, Converters, Behaviors, Controls, Resources |
| `POS.Api/` | ASP.NET Core Web API reusing `BLL` + `DAL` |
| `Mobile/` | Expo / React Native manager companion app |
| `BLL.Tests/` | xUnit + FluentAssertions unit tests |
| `docs/` | Architecture, requirements, API contract, and offline-strategy docs |

### Key architectural rules

- **EF Core is the default ORM**; ADO.NET is used only for bulk operations,
  reporting, or performance-critical paths.
- The **UI/ViewModel never chooses the data-access technology** and never
  constructs a `DbContext` or `SqlConnection` directly.
- **No EF/ADO.NET mixing** inside a single repository method.
- **No ORM entities leak past the DAL boundary** — data is projected to DTOs /
  read models.
- `POS.Api` controllers stay thin, calling BLL service interfaces directly —
  no extra application-service layer between Controller and BLL.

---

## Database

The SQL Server schema is defined as raw SQL scripts under `DAL/db/`:

```
DAL/db/
├── Tables/            # CREATE TABLE scripts (dbo.*.Table.sql)
├── Views/             # view definitions
├── StoredProcedures/  # stored procedures
├── Functions/         # scalar / table-valued functions
├── Indexes/           # index definitions
├── Seeds/             # seed data
├── UserDefinedTypes/  # custom types
└── Migrations/        # EF Core migrations
```

The EF Core `DbContext` and entity models live under `DAL/Entities/` and
`DAL/Configurations/`.

Key tables include: `Products`, `Categories`, `Sizes`, `ModifierGroups`,
`ModifierOptions`, `Transactions`, `TransactionItems`, `TransactionItemModifiers`,
`Payments`, `PurchaseReceipts`, `PurchaseReceiptTypes`, `ReceiptCounters`,
`Shifts`, `Sessions`, `Users`, `Roles`, `Suppliers`, `TaxRates`, `AuditLogs`,
plus per-table translation tables (e.g. `ProductTranslations`).

---

## Project Configuration

**WPF / API**
- `WPF/appsettings.json` — connection string.
- `WPF/shortcuts.json` — configurable keyboard shortcuts.
- `WPF/Assets/` — icons and images (logo, nav icons, illustrations).

**Mobile**
- `Mobile/.env.example` — API base URL and other environment values (actual
  values are not committed).
- `Mobile/src/api/` — axios client, base-URL config, Bearer-token interceptor.
- `Mobile/src/i18n/` — localized UI strings.

---

## License

This project is proprietary / internal — see the repository owner for
licensing details.
