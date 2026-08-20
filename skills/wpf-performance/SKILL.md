---
name: wpf-performance
description: Defines the concrete, checkable performance patterns for WPF — async/threading discipline (no blocking calls, no stray `async void` outside `AsyncRelayCommand`), when UI virtualization is actually warranted, and `ObservableCollection` update discipline. Use when a screen is reported as slow/laggy, when adding a list/grid that could grow large, or before merging any change with a loop doing per-item UI or per-item DB work. This is deliberately narrow — real, checkable rules grounded in what's already in the codebase, not general performance advice.
---

# WPF Performance

## When to use this

A screen feels slow, freezes, or stutters; you're adding a list/grid that could realistically hold hundreds+ rows; or you're reviewing a change that loops over data doing UI updates or BLL/DB calls per item. Don't reach for this proactively on every change — most WPF screens in this app are small management lists that don't need any of this. Apply it when there's a real, specific reason to.

## Async/threading discipline

- **Never block on async code** — no `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` anywhere in a WPF ViewModel or code-behind. On the WPF UI thread this risks a deadlock (the classic WPF/`SynchronizationContext` async deadlock), not just a stall. `AsyncRelayCommand` (see `wpf-mvvm-viewmodel`) exists specifically so commands can be properly async without a caller needing to block — always route an async operation through it rather than wrapping it synchronously.
- **`async void` is only acceptable in one place**: `ICommand.Execute` implementations, where the interface itself requires a `void` return (see `AsyncRelayCommand.Execute` — this is the sanctioned, unavoidable case, with its own doc-comment explaining why an unhandled exception bubbles to the app's global handler rather than being awaitable by the caller). **Don't write a new `async void` method anywhere else** — an event handler, a helper method, a `Save()`/`OnReportAction()`-style method — even though a couple of pre-existing ones exist in the codebase outside `AsyncRelayCommand` (legacy — don't copy them, and don't "fix" them as a drive-by per `AGENTS.md` Scope Discipline). A new async method should always return `Task`, wired up through `AsyncRelayCommand`, so exceptions are observable and the caller can actually await it if it ever needs to.
- **WPF dialogs must run on the UI/STA thread.** If a command needs to do real CPU-bound work (a large export, a heavy computation) alongside showing a `SaveFileDialog` or similar, capture what the dialog needs *before* offloading the CPU-bound part to a background thread — see `ReportViewModel.Commands.cs`'s Excel export for the existing pattern (show the save dialog first, synchronously, then do the async export work). Don't try to show a WPF dialog from a non-UI thread.
- **Don't do BLL/DB calls inside a UI-bound loop.** If a screen needs data for N rows, get it in one batched BLL call (see `bll-dal-service-creation`'s N+1-avoidance guidance, e.g. `CategoryService`'s batch-loaded translations dictionary) rather than looping over rows and awaiting a service call per row — that pattern is already established on the BLL side; don't reintroduce a per-row round trip from the WPF ViewModel that consumes it.

## UI virtualization — only where it's actually warranted

- `VirtualizingWrapPanel` (the third-party package already a dependency) is used today in exactly one place: `ProductsPanelView` (Cashier product grid) — the one screen in the app where the item count is large and dynamic enough (the full product catalog) to matter. This is the right scope for it: **don't add `VirtualizingWrapPanel` to every `ItemsControl`/list by default** — it adds complexity (custom panel, potential sizing quirks) that isn't justified for a management screen's list of a few dozen categories or sizes.
- `DataGrid` virtualizes rows by default in WPF (`EnableRowVirtualization="True"` is the framework default) — don't explicitly set it `False` unless there's a specific, stated reason (e.g. a grid that needs every row realized for some interaction), since disabling it is what actually causes a large grid to become slow.
- If a *new* screen's list could realistically grow into the hundreds of items and isn't a `DataGrid` (e.g. another `ItemsControl`-based card/grid layout like the product grid), that's the signal to consider `VirtualizingWrapPanel` or an equivalent virtualizing panel — not a default to reach for on every new list.

## `ObservableCollection` discipline

`ObservableCollection<T>` is used throughout for bindable lists (25+ usages across ViewModels). Each `Add`/`Remove`/`Clear` raises a `CollectionChanged` notification that the UI reacts to individually:

- **When replacing a whole list's contents** (e.g. after `RefreshDataAsync()` reloads from the BLL), clear and repopulate rather than leaving stale items partially updated — but be aware that `ObservableCollection.Clear()` + a loop of `Add()` calls fires one notification per operation. For a list large enough that this is measurably slow (a genuinely large result set, not a typical management screen's few dozen rows), assign a **new** `ObservableCollection<T>` to the bound property instead of mutating the existing one in a loop — one `PropertyChanged` notification instead of N `CollectionChanged` notifications. For the list sizes typical of this app's management screens, the simple clear-and-repopulate loop is fine and matches existing code; don't add complexity here without a measured reason.
- Don't rebuild a whole `ObservableCollection` on every keystroke of a search/filter box if the underlying data hasn't changed — filter a separate view of the existing data (e.g. `ICollectionView`/`CollectionViewSource`, or a filtered projection) rather than re-querying the BLL or reconstructing the source collection per keystroke. If a screen's search-as-you-type already re-queries the BLL per keystroke, that's worth flagging, not necessarily changing without being asked — see `AGENTS.md` Scope Discipline.

## Investigating a reported slow screen

1. Confirm it's actually a WPF-side issue and not a slow BLL/DB query — check whether the BLL call itself is slow (test the endpoint/method in isolation) before assuming the UI layer is at fault. If it's the query, that's `bll-dal-service-creation`/DAL territory, not this skill.
2. Check for the async/threading anti-patterns above — a blocking `.Result`/`.Wait()` or a synchronous loop of awaited per-item calls is the most likely real cause of a "frozen UI" complaint specifically (as opposed to "slow to load").
3. Check whether the screen's list/grid size is large enough that virtualization is actually the lever to pull — most reported slowness in a POS management screen with realistic data volumes won't be a virtualization problem; don't reach for `VirtualizingWrapPanel` as a first guess without checking the actual row count.
4. Make the smallest change that addresses the measured cause — this skill exists to give you the checkable causes, not license to restyle or restructure the screen while you're in there (`AGENTS.md` Rule 11, Scope Discipline).

## Mid-sized WPF performance workflow for this app

For this POS app, the most common "WPF is freezing" report is really a **critical-path sequencing** problem, not an enterprise-scale rendering problem:

- **Trace before changing architecture.** Add short-lived stopwatch traces around the distinct stages the user feels: auth/query time, CPU verification time, shell/window resolution, initial dashboard section loads, and any best-effort hydration after shell open. The professional first move here is to split one 5-second complaint into named segments, not to guess at virtualization or broad refactors.
- **Treat login/shell-open as its own latency budget.** Only the work strictly required to authenticate and show the first frame belongs on that path. Shift hydration, dashboard refreshes, recent-sales loads, and similar secondary work should happen after the shell is visible unless the UI literally cannot render without them.
- **Suspect query shape before suspecting WPF.** On this codebase's typical data sizes, a slow username lookup or a synchronous DbContext/query path is more likely than the visual tree itself. Check for predicates that defeat indexes (`Trim()`, `ToLower()`, client-shape surprises) before reaching for bigger UI changes.
- **Constructor work is part of the freeze.** If a ViewModel constructor starts service calls, those calls effectively happen on navigation/login critical path. For first-visit data, prefer the established `EnsureDataLoadedAsync()` / `EnsureInitializedAsync()` pattern, optionally with a one-turn `Task.Yield()` so the shell can paint before background loading begins.
- **Run independent dashboard sections together.** If KPI, recent-transactions, top-products, and similar sections don't depend on each other, start them together with one `Task.WhenAll(...)` rather than serial awaits. In this app, that is the preferred "mid-sized" optimization before any heavier redesign.

## Don't

- Don't write a new `async void` method outside an `ICommand.Execute` implementation.
- Don't block on async code with `.Result`/`.Wait()` anywhere in WPF.
- Don't add `VirtualizingWrapPanel` (or disable `DataGrid` row virtualization) without a concrete large-list reason — it's not a default to apply everywhere.
- Don't loop over rows awaiting a BLL call per row when a batched call is possible — check `bll-dal-service-creation`/existing service methods for a batch-loading pattern first.
- Don't "optimize" a screen that wasn't reported as slow and isn't handling a realistically large dataset — this skill is for real, specific performance work, not speculative tuning.

## Related files

`skills/wpf-mvvm-viewmodel/SKILL.md` (`AsyncRelayCommand`, the sanctioned `async void` case), `skills/bll-dal-service-creation/SKILL.md` (N+1-avoidance / batch-loading on the BLL side), `skills/cross-surface-impact-check/SKILL.md` (if a fix changes a BLL method's shape, not just the WPF caller), `AGENTS.md` Rule 11 and Scope Discipline.
