---
name: wpf-view-creation
description: Defines how to add a non-CRUD WPF screen — reports, dashboards, home/summary pages, read-only detail views. Covers file placement, View/ViewModel pairing, and folder structure. For screens with create/update/delete, use `wpf-crud-screen` instead — check that first, since a screen that looks like a simple list often turns out to need CRUD. For ViewModel internals (BaseViewModel, commands, DI) see `wpf-mvvm-viewmodel`; for making the screen reachable see `wpf-navigation`.
---

# WPF View Creation (non-CRUD)

## When to use this

A new WPF screen that doesn't add/edit/delete records — a report, a dashboard summary (`HomeViewModel`), a read-only breakdown, an export/filter view (`ReportViewModel`). **Before starting, check whether the screen actually needs CRUD** — a "list of X" request often turns into "list of X with add/edit/delete" once requirements are fully understood; if so, use `wpf-crud-screen` instead of this skill. When genuinely unsure, ask rather than build the read-only version and redo it as CRUD later.

## File placement

Mirrors the existing per-feature folder structure — don't invent a new top-level grouping:

- ViewModel: `WPF/ViewModels/<Area>/<Name>ViewModel.cs` (e.g. `WPF/ViewModels/Reports/ReportViewModel.cs`). If the ViewModel is genuinely large and covers distinct concerns (filtering vs. exporting vs. loading, as `ReportViewModel` likely does given its size), split into `partial class` files the same way `wpf-mvvm-viewmodel`'s SOLID section describes for CRUD screens — `<Name>ViewModel.cs` for the core, `<Name>ViewModel.<Concern>.cs` for each split-out concern.
- View: `WPF/Views/<Area>/<Name>View.xaml` + code-behind, matching the ViewModel's area folder.
- If the screen is a genuinely new top-level area (not an existing folder like Reports/Products/Categories), create the matching folder in both `ViewModels/` and `Views/` — don't drop a new screen into an unrelated existing folder because it's "close enough."

## Structure

- Inherit `BaseViewModel` (see `wpf-mvvm-viewmodel` for what this gives you — `RunAsync<T>`, `OnPropertyChanged`, `Dispose(bool)`).
- Constructor takes injected service interfaces only (`IReportService`, `ILocalizationService`, etc.) — no runtime-only parameters, since non-CRUD screens are typically constructed once via DI/navigation, not via `IViewModelFactory` with per-instance data (that's the CRUD form-dialog pattern, not this one). If a non-CRUD screen genuinely needs runtime-only construction data, that's a signal to double check it isn't actually dialog-shaped, in which case treat it like the form side of `wpf-crud-screen` instead.
- Apply the same lazy-load pattern as CRUD list screens (`_hasLoadedOnce`/`EnsureDataLoadedAsync`, see `wpf-mvvm-viewmodel`) if the screen loads data from the BLL and is reached via the `ManagerMainViewModel.CurrentPage` navigation mechanism — see `wpf-navigation`. A screen like `ReportViewModel` that's filter-driven (user picks a date range, then loads) may instead load on an explicit filter-change command rather than eagerly — match whichever of these two shapes fits how the screen is actually used, but don't load in the constructor either way.
- Use `Contracts/Enum/*` types for any status/mode enum that's shared with the API layer (e.g. `TransactionStatus`) rather than defining a parallel WPF-only enum for the same concept — but a genuinely WPF-local UI concern (like `ReportViewModel`'s `ReportFilterMode: Today/Week/Month/Period`, which is about how the report screen's filter UI behaves, not a business/domain concept) is fine to define locally in the ViewModel file, since it has no meaning outside this screen.

## Code-behind

Same discipline as `wpf-mvvm-viewmodel` states generally: `InitializeComponent()` only, no logic.

## Wiring it up

1. Register the ViewModel `AddTransient` in `App.xaml.cs` (see `wpf-mvvm-viewmodel`'s DI section).
2. Follow `wpf-navigation` to make it reachable — almost always mechanism 2 (`ManagerMainViewModel.CurrentPage` + `DataTemplate`), same as CRUD screens.
3. Follow `wpf-styling` for chrome/layout consistency (`DashboardChrome.xaml`, `Resources/Common/*`) rather than one-off styling.
4. If the screen displays a shared reusable control (`CurrencyText`, `DateRangeFilterControl`, etc.), use it — see `wpf-reusable-controls` — rather than rebuilding the same UI inline.

## Don't

- Don't build a "read-only for now, we'll add CRUD later" screen without checking `wpf-crud-screen` first — retrofitting CRUD onto a screen not structured for it (no partial-class split, no `IViewModelFactory`-constructed form ViewModel pattern in place) is more work than starting with the right shape.
- Don't load data in the constructor — see `wpf-mvvm-viewmodel`'s lazy-load section; this applies to non-CRUD screens exactly as much as management screens.
- Don't invent a new folder-naming convention under `ViewModels/`/`Views/` — match the existing per-feature-area pattern.

## Related files

`skills/wpf-crud-screen/SKILL.md` (if the screen turns out to need mutations), `skills/wpf-mvvm-viewmodel/SKILL.md` (ViewModel internals), `skills/wpf-navigation/SKILL.md` (making it reachable), `skills/wpf-styling/SKILL.md` (chrome/layout), `skills/wpf-reusable-controls/SKILL.md` (shared controls to reuse).
