# DAL Guidelines

Date: 2026-08-17

This guide gives a simple default for choosing between EF Core and ADO.NET in the POS DAL.

## Default rule

- Use EF Core for ordinary CRUD, detail views, and entity-shaped workflows.
- Use ADO.NET for proven hot paths, stored-procedure-heavy work, and places where measurement shows EF overhead is material.
- Prefer one data-access style per repository unless there is a clear transactional or performance reason to mix them.

## Quick decision table

| Situation | Choose | Why |
|---|---|---|
| Create, edit, delete a single entity | EF Core | Clear, maintainable, and easy to keep consistent |
| Load a detail screen with related entities | EF Core | Good fit for tracked or no-tracking entity graphs |
| Read-only list, dashboard, or report with a known SQL shape | ADO.NET or projection-first EF | Better control over payload size and query cost |
| Stored procedure already exists and is the contract | ADO.NET | Avoids extra ORM work and maps cleanly to the procedure |
| Login/auth or other measured hot path | ADO.NET | Keeps the critical path lean |
| Write flow needs one transaction with multiple operations | Either, based on shape | Use the simplest approach that preserves atomicity and readability |

## How to choose

1. Start with EF Core if the code is standard CRUD or detail access.
2. Switch to ADO.NET only after measurement, or when the operation is already naturally SQL-shaped.
3. If a repository starts mixing both styles, ask whether the mixed design is really necessary or whether the hot path should be split out.

## Read rule

- Default to no-tracking for read-only queries.
- Use tracking only when the same context will intentionally modify the loaded entity.

## Write rule

- Keep write logic explicit.
- Avoid hiding multiple writes behind generic one-row repository calls when the business operation wants a single command boundary.

## Examples from this codebase

- `UserRepository.GetByUsernameAsync()` uses ADO.NET because it calls `SP_LoginUser`.
- `TransactionRepository.GetTransactionsListAsync()` uses ADO.NET because it is a stored-procedure-backed listing path.
- `Repository<T>` uses factory-created short-lived EF contexts for standard CRUD.

## Measurement rule

- Do not change a repository from EF to ADO.NET just because it feels faster.
- Measure first, keep the code easy to review, and only optimize where the data says it matters.
