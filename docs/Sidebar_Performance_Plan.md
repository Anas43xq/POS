# Sidebar Performance Plan

Date: 2026-08-17

Goal: make manager sidebar switching feel faster without changing bindings, UI behavior, or unrelated business logic.

## Phase 1: Remove redundant page work on reselect

- Make manager sidebar activation idempotent.
- If the requested page is already active, do not reassign `CurrentPage`.
- Keep each page's existing first-load behavior intact.
- Preserve explicit refresh commands inside the page view-models.

## Phase 2: Keep page loads lazy and parallel where they already are

- Continue using one-time `EnsureDataLoadedAsync()` patterns for pages that only need to load once.
- Keep independent dashboard sections loading together with `Task.WhenAll(...)`.
- Do not add `Task.Run` around EF or SQL work.

## Phase 3: Measure before changing more

- Add short stopwatch traces only where a switch still feels slow.
- Split switch time into shell selection, first paint, and data load if needed.
- Only add more concurrency if two page loads are truly independent.

## Guardrails

- No WPF binding changes.
- No new packages.
- No speculative threading changes for EF or SQL.
- Keep any future optimization easy to measure afterward.
