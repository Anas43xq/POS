---
name: wpf-mvvm-viewmodel
description: Defines how to write or change a WPF ViewModel/View pair in this project — the homegrown MVVM pattern (`BaseViewModel`, `RelayCommand`/`AsyncRelayCommand`, `IViewModelFactory`, `IDialogService`), code-behind discipline, lazy data-loading, DI registration, and how to make a ViewModel actually testable given there's no unit-test project for WPF yet. Use whenever adding or modifying a WPF View, ViewModel, dialog, or anything under `WPF/ViewModels`/`WPF/Views`. Governs *how* to structure WPF code; `AGENTS.md` Rule 2 ("don't rewrite what already works") governs *whether* you should be touching WPF at all for a given task.
---

# WPF MVVM / ViewModel

## When to use this

Any new or changed WPF ViewModel, View code-behind, dialog, or command. Read this before writing one from scratch or extending an existing one — don't invent a different pattern (e.g. reaching for `CommunityToolkit.Mvvm`, `INotifyPropertyChanged` boilerplate by hand, or code-behind event handlers calling services directly) when this project already has one established.

## The pattern this project actually uses

This is a **homegrown MVVM implementation**, not a third-party MVVM framework — no `CommunityToolkit.Mvvm`, no Prism, no ReactiveUI in the `.csproj`. Don't add one (`AGENTS.md` Rule 6: justify new dependencies — the existing pattern already does the job for every current ViewModel).

- **`UI.ViewModels.BaseViewModel`** (`WPF/ViewModels/Core/BaseViewModel.cs`) — every ViewModel inherits from this. Gives you:
  - `INotifyPropertyChanged` via `OnPropertyChanged([CallerMemberName])` — call it in every property setter that should update the UI.
  - `RunAsync<T>(operation, onSuccess)` — the standard way to call a BLL method returning `Result<T>`: runs it, calls `onSuccess` on success, surfaces `result.Error` through `Notifications` (an injected `INotificationService`) on failure. Use this instead of hand-rolling try/catch-and-show-message-box around every BLL call that returns `Result<T>`.
  - `IDisposable` with a `Dispose(bool)` override point — override this to unsubscribe from any singleton-published event (the recurring case: `ILocalizationService.LanguageChanged`). A transient ViewModel that subscribes to a singleton's event and never unsubscribes is permanently rooted and never garbage-collected — this is the actual reason the override point exists, not just convention.
- **Commands**: `UI.Commands.RelayCommand` / `AsyncRelayCommand` (`WPF/Commands/`) — construct in the ViewModel constructor: `new RelayCommand(Method, () => CanExecuteCondition)`, `new AsyncRelayCommand(AsyncMethod, () => CanExecuteCondition)`. Use `AsyncRelayCommand` for anything that awaits (a BLL call); `RelayCommand` for synchronous UI-only actions (start-edit, cancel-edit). Don't make a command `async void` by hand — that's exactly what `AsyncRelayCommand` exists to avoid.
- **`IViewModelFactory.Create<T>(params object[] parameters)`** (`WPF/Services/`) — use this to construct a dialog/child ViewModel that needs both DI-resolvable services *and* runtime-only data (the record being edited, a parent reference, a completion callback). Don't `new` a ViewModel by hand when it has any constructor dependency that should come from DI — that bypasses the container and silently diverges from how every other ViewModel is built.
- **`IDialogService`** — for opening dialogs/message boxes from a ViewModel without giving the ViewModel a direct WPF `Window`/`MessageBox` dependency, which would make it untestable (see Testability below) and violate MVVM's "ViewModel doesn't know about View types" boundary.

## Code-behind discipline

View `.xaml.cs` files in this project are consistently thin — see `SizeManagementView.xaml.cs`: constructor calls `InitializeComponent()` and nothing else. Match this:

- No business logic, no BLL/service calls, no direct manipulation of ViewModel internals from code-behind.
- The only things that belong in code-behind are things that genuinely can't be done from XAML/ViewModel — a control-specific event that has no MVVM-friendly equivalent (rare), or view-only concerns like focus management.
- If you find yourself adding a non-trivial method to a `.xaml.cs` file, that's a signal the logic belongs in the ViewModel instead — move it there before finishing the change.

## Lazy data loading — established, don't reinvent per-screen

Every management ViewModel (`ProductManagementViewModel`, `SizeManagementViewModel`, and others) follows the same load pattern: data is **not** loaded in the constructor. Instead:

- A `_hasLoadedOnce` flag plus an `EnsureDataLoadedAsync()` method that no-ops on subsequent calls.
- The navigation layer (`ManagerMainViewModel.Navigate...()`) calls `EnsureDataLoadedAsync()` the first time a page is navigated to.
- A separate `RefreshCommand` (`AsyncRelayCommand(LoadDataAsync)`) lets the user force a reload later.
- If a login or page-switch path feels frozen, treat constructor-started work as suspect by default. Keep constructor work cheap, let the shell/page show first, and only then start non-critical loads via `EnsureDataLoadedAsync()` / `EnsureInitializedAsync()`. A single `await Task.Yield()` before the first real load is an acceptable pattern here when it lets WPF paint the shell before dashboard data work begins.

Follow this for any new management-style screen rather than loading eagerly in the constructor — eager constructor loading would fire a BLL/DB call before the ViewModel is even navigated to, and DI-container construction timing for `AddTransient` ViewModels doesn't guarantee that maps to "user is about to see this screen."

## Wiring a new ViewModel into DI

Two things, both required, in `WPF/App.xaml.cs`:

1. `services.AddTransient<YourViewModel>();` — ViewModels are `AddTransient`, not `AddSingleton` (a singleton ViewModel would retain stale state and stay rooted in memory across navigations — the `Dispose(bool)` unsubscribe pattern above assumes transient lifetime). Long-lived cross-cutting services (`INavigationService`, `IViewModelFactory`, `INotificationService`, `ILocalizationService`, etc.) are the ones registered `AddSingleton` — don't flip a ViewModel to singleton to "keep its state" between visits; use `EnsureDataLoadedAsync`/`RefreshCommand` instead.
2. If the ViewModel is a dialog or takes runtime-only constructor parameters, it's created via `IViewModelFactory.Create<T>(...)` at the call site instead of resolved directly — it still needs the `AddTransient` registration so the factory can resolve its DI-only constructor parameters, but callers don't pull it straight from the container.

## Testability — what's realistic given no WPF test project exists today

There is no `WPF.Tests` project (unlike `BLL.Tests` — see `backend-testing-conventions`). Don't add one speculatively for a single ViewModel change (`AGENTS.md` Rule 6). What you can and should do without a test project:

- **Keep ViewModels testable in principle** even though nothing tests them yet: depend on interfaces (`ISizeService`, `IDialogService`, `INotificationService`), not concrete classes or static calls, so a future test project could construct a ViewModel with fakes. This is the existing pattern in every ViewModel shown above — don't break it by reaching for a static service locator or a `new ConcreteService()` inside a ViewModel.
- **Push real logic out of the ViewModel and into the BLL where it already belongs** — a ViewModel should orchestrate (call a service, map the result to bindable properties, react to user actions), not contain business rules. Business logic in a ViewModel is both a layering violation (`AGENTS.md` Rule 4's spirit applies here too, even though that rule is phrased for controllers) and untestable today, since nothing exercises WPF ViewModels. If a ViewModel's method has real branching worth verifying, ask whether it's actually BLL logic that's ended up in the wrong layer — see `bll-dal-service-creation`.
- If a future task explicitly asks to add WPF ViewModel unit tests, that's new project scaffolding (a `WPF.Tests` project, likely xUnit + FluentAssertions to match `BLL.Tests`' conventions) — flag it as a real addition needing its own setup, not something to bootstrap quietly inside an unrelated change.

## SOLID / clean-code, applied to ViewModels specifically

Everything in `skills/code-style/SKILL.md` applies as-is (function naming, comment discipline, no over-engineering). A few WPF-specific extensions:

- **Single Responsibility per ViewModel**: a management ViewModel that's grown multiple concerns (e.g. `ProductManagementViewModel` splitting into `.Crud.cs`/`.Tree.cs` partial-class files) is already the established way to keep one class's *file* from becoming unwieldy while it's still conceptually one ViewModel — use `partial class` splits like this for a large existing ViewModel rather than either (a) letting one file grow past readability, or (b) fully decomposing it into multiple unrelated ViewModel classes that then have to coordinate state between them.
- **Don't let a ViewModel reach into another ViewModel's internals directly.** Where one ViewModel genuinely needs to trigger something in another (e.g. a dialog completing and needing to refresh a parent list), pass a callback via `IViewModelFactory.Create<T>` parameters, matching the existing dialog pattern — don't give a parent ViewModel a public mutable property that a child reaches up and sets directly.
- **Commands should call a plainly-named method, not inline lambdas with real logic** — `new AsyncRelayCommand(SaveAsync)`, not `new AsyncRelayCommand(async () => { /* 15 lines */ })`. Keeps the constructor scannable and keeps the actual logic testable/readable as a named method.

## Don't

- Don't add `CommunityToolkit.Mvvm` or any other MVVM package — the existing `BaseViewModel`/`RelayCommand` pattern already covers this project's needs; see `AGENTS.md` Rule 6.
- Don't put BLL/DAL calls, `MessageBox.Show`, or file/dialog I/O directly in code-behind — route through the ViewModel and injected services.
- Don't register a new ViewModel as `AddSingleton` to preserve state across navigation — use the lazy-load/`EnsureDataLoadedAsync` pattern instead.
- Don't skip overriding `Dispose(bool)` when a ViewModel subscribes to a singleton-published event — it's the specific, real reason that override point exists, not boilerplate to skip.
- Don't redesign an existing View's layout/visual design while changing its ViewModel unless the task explicitly asks for it (`AGENTS.md` Rule 11).

## Related files

`AGENTS.md` Rules 2, 4 (spirit), 6, 7, 11; `skills/code-style/SKILL.md` (general code style, applies alongside this); `skills/bll-dal-service-creation/SKILL.md` (where real business logic belongs, not in the ViewModel); `skills/backend-testing-conventions/SKILL.md` (the testing conventions that would extend to WPF if a `WPF.Tests` project is ever added); `skills/cross-surface-impact-check/SKILL.md` (WPF is a direct BLL consumer — check impact before changing a BLL interface a ViewModel depends on).
