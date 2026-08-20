---
name: wpf-navigation
description: Defines the two distinct navigation mechanisms in the WPF app — INavigationService (full ViewModel swap with dispose) and the ManagerMainViewModel.CurrentPage tab-like pattern (no dispose, lazy load) — and how to register a new destination in each. Use whenever adding a new page/screen that needs to be reachable, or a new dialog that needs to open from an existing screen. Does not cover what the screen itself contains — see `wpf-crud-screen` or `wpf-view-creation` for that.
---

# WPF Navigation

## When to use this

Any time a new screen needs to become reachable from somewhere in the app, or an existing navigation path needs to change. Read this before adding a new `NavigateTo...()` method, a new `DataTemplate` mapping, or wiring up a new dialog launch.

## Two mechanisms — don't conflate them

This app has two genuinely different navigation systems serving different purposes. Picking the wrong one for a new screen produces subtly broken behavior (stale data on revisit, or a ViewModel never getting disposed).

### 1. `INavigationService.NavigateTo<T>()` — full swap, disposes outgoing VM

`WPF/Services/INavigationService.cs` / `NavigationService.cs`. Used for switching between **top-level app shells** (e.g. Cashier dashboard ↔ Manager dashboard) — a small number of call sites, not per-screen navigation within a dashboard.

- Resolves the target ViewModel fresh from DI (`AddTransient`, so a new instance every time) and sets it as `CurrentViewModel`.
- **Explicitly disposes the outgoing ViewModel** if it implements `IDisposable`, after the new one is wired up and the UI notified — this is what makes overriding `Dispose(bool)` in `BaseViewModel` (see `wpf-mvvm-viewmodel`) actually matter for anything navigated to this way: an outgoing VM subscribed to `ILocalizationService.LanguageChanged` gets unsubscribed here.
- Use this only for genuinely top-level, infrequent transitions — not for switching between management pages inside the Manager dashboard, which uses mechanism 2 instead.

### 2. `ManagerMainViewModel.CurrentPage` + `DataTemplate` — tab-like, no dispose

`WPF/ViewModels/Dashboards/ManagerMainViewModel.Navigation.cs` + `DataTemplate` mappings in `WPF/Views/Main/ManagerMainView.xaml`. Used for switching between pages **within** the Manager dashboard shell (Products, Categories, Sizes, Reports, Transactions, etc.) — this is what almost every new management/report screen should use.

- Each destination ViewModel (`_productManagementViewModel`, `_categoryManagementViewModel`, etc.) is injected once into `ManagerMainViewModel`'s constructor and held for the lifetime of the dashboard — **not** re-resolved from DI on every navigation, unlike mechanism 1.
- A `NavigateTo<Name>()` method just does `CurrentPage = _xViewModel;` and, for lazy-loading screens, kicks off `_ = _xViewModel.EnsureDataLoadedAsync();` (see `wpf-mvvm-viewmodel`'s lazy-load section) without awaiting — the page renders immediately and data populates once loaded.
- No `Dispose()` call on navigating away — the previous page's ViewModel is simply not the current one; it stays alive (held by `ManagerMainViewModel`) until the dashboard itself closes. This is why `EnsureDataLoadedAsync`'s `_hasLoadedOnce` guard matters: navigating away and back must not silently drop cached state or wastefully reload.
- The `DataTemplate DataType="{x:Type vm:XViewModel}"` mapping in `ManagerMainView.xaml` is what actually renders `CurrentPage` as the right View — a new page's ViewModel **must** have a matching `DataTemplate` entry or it will render as a blank/default content presenter with no visible error.

## Registering a new page (mechanism 2 — the common case)

1. Add a field + constructor parameter for the new ViewModel on `ManagerMainViewModel` (injected via DI, so it must already be `AddTransient`-registered per `wpf-mvvm-viewmodel`'s DI section — though held as a singleton-per-dashboard-instance in practice since `ManagerMainViewModel` only constructs it once).
2. Add `NavigateTo<Name>()` in `ManagerMainViewModel.Navigation.cs`, following the `CurrentPage = _xViewModel; _ = _xViewModel.EnsureDataLoadedAsync();` shape (omit the `EnsureDataLoadedAsync` call if the screen doesn't lazy-load, e.g. `NavigateToReports()`).
3. Wire whatever triggers navigation (a sidebar button command) to call `NavigateTo<Name>()`.
4. Add the `DataTemplate DataType="{x:Type vm:XViewModel}"` entry in `ManagerMainView.xaml`, pointing at the matching View.
5. Don't forget step 4 — a missing `DataTemplate` is the most common way a new page "does nothing" when navigated to, with no exception thrown.

## Registering a new dialog

Dialogs (form dialogs, confirmation dialogs beyond a simple `MessageBox`, translation dialogs) don't go through either navigation mechanism above — they go through `IDialogService.ShowDialog<TView>(viewModel)`, constructed via `IViewModelFactory.Create<T>(...)`. See `wpf-crud-screen` for the full add/edit dialog pattern; this skill's job is just to make clear dialogs are a separate concern from page navigation, not a third variant of it.

## Choosing between the two mechanisms for a new screen

- **New management/report/detail page reachable from inside an existing dashboard** → mechanism 2 (`ManagerMainViewModel.CurrentPage` + `DataTemplate`). This is almost every new screen.
- **New top-level shell** (a genuinely new mode of the app, parallel to Cashier/Manager) → mechanism 1 (`INavigationService`). Rare — confirm this is really what's being asked before reaching for it, since it's a much bigger structural addition than a new dashboard page.
- If unsure which applies, look at what already exists at the same conceptual level as the new screen and match its mechanism — don't introduce a third pattern.

## Don't

- Don't call `INavigationService.NavigateTo<T>()` for switching between pages inside the Manager dashboard — that re-resolves the ViewModel from DI every time (losing `_hasLoadedOnce` state) and doesn't match how sibling pages behave.
- Don't add a new `NavigateTo<Name>()` to `ManagerMainViewModel.Navigation.cs` without adding the matching `DataTemplate` in the same change — they must land together.
- Don't call `EnsureDataLoadedAsync()` synchronously with `await` from a navigation method — the existing pattern fires it with `_ =` (discarded) so the page renders immediately rather than blocking navigation on the load.

## Related files

`skills/wpf-mvvm-viewmodel/SKILL.md` (lazy-load pattern, `Dispose(bool)`), `skills/wpf-crud-screen/SKILL.md` (dialog launch pattern), `skills/wpf-view-creation/SKILL.md` (what a new non-CRUD page contains before it's wired up here).
