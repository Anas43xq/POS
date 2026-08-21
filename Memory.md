# Memory.md - WPF POS

Durable handoff for the WPF POS app. The visual-layer rebuild described in `AGENTS.md` is complete; this file now tracks the current state, locked rules, and open items for ongoing work. Keep this short, factual, and current.

Detailed known issues and historical fixes live in `KownIssues.md`. Check that file before debugging XAML runtime/build errors or reintroducing old resource aliases.

## Project

- `WPF/` visual-layer rebuild is complete: all view files under `WPF/Views/` (Auth, Cashier, Categories, Dialogs, Main, Modifiers, Products, Reports, Sales, Settings, Shifts, Sizes, Transactions) run on the token/control/chrome system.
- ViewModels, BLL/DAL, bindings, navigation, converters, custom controls, localization, and backend were untouched during the rebuild and remain governed by their own conventions (see `skills/wpf-mvvm-viewmodel/SKILL.md`, `skills/bll-dal-service-creation/SKILL.md`, etc.) rather than the styling rules.
- Ongoing goal: any new screen or edit stays token-based with zero hardcoded visual values, per `AGENTS.md`.

## Current status

- Rebuild phases 0–6 (`AGENTS.md`) are complete.
- Verification pass on 2026-08-20 against the checklist in `AGENTS.md`: `Foreground=`, `Background=`, inline hex, `Color="{StaticResource` type mismatches, and culture-based currency formatting all returned 0 hits in `WPF/Views/`. `FontSize=`/`Margin="[0-9]` hits are confined to `ReceiptWindow.xaml`/`ReceiptPrintView.xaml`, which is the documented receipt exception.
- **Open item**: `ShiftManagementView.xaml` has four inline `<Style TargetType="DataGridColumnHeader" BasedOn="{StaticResource POS.DataGrid.ColumnHeader}">` blocks inside `DataGridTemplateColumn.HeaderStyle`, each adding one `HorizontalContentAlignment="Right"` setter. This is a local `<Style>` block, which `AGENTS.md`'s hard rules disallow — it wasn't caught before. Not fixed in this pass since it wasn't the task in scope; flagging so the next WPF task either moves this into a named style in `Themes/Controls/DataGrid.xaml` (e.g. `POS.DataGrid.ColumnHeader.Right`, already referenced conceptually in `KownIssues.md`) or gets an explicit decision to leave it as a narrow, justified exception.

## Locked rules

- Token namespaces: `Color.X.Y`, `Font.X.Y`, `Spacing.N`, `Size.X.Y`, `Radius.X`, `Shadow.X`
- Control style namespaces: `POS.Button.X`, `POS.TextBox.Default`, `POS.ComboBox.Default`, `POS.DataGrid.Default`
- Primary brand color: `#2589E0`
- Spacing base: 4px
- Minimum touch target: 44px
- Currency in views: use `controls:CurrencyText` for totals, payment panels, receipts, KPI summaries, and standalone money displays; dense list/table row amounts use plain numeric formatting with no currency glyph or symbol
- List/check-list/DataGrid rows must bind explicit display fields/templates; never let row content/details presenters fall back to object `ToString()` namespace text
- ViewModel reload paths that reuse `_loadCts` must clear the field before disposing the active `CancellationTokenSource`; never leave `_loadCts` pointing at a disposed instance
- Inline hex allowed only in `Colors.xaml` and `Shadows.xaml`
- `DropShadowEffect.Color` must use inline 6-digit color values; do not bind it to brush resources or 8-digit alpha hex
- `Foreground` belongs to style setters only, never on button elements or inner text
- DAL persistence rule: use EF Core by default for CRUD and detail workflows; use ADO.NET only for proven hot paths, stored-procedure-heavy operations, or places where measurement shows EF overhead is material. Prefer one data-access style per repository unless there is a clear transactional or performance reason to mix them.
- `docs/DAL_Guidelines.md` now captures the short EF-vs-ADO decision table and examples for future DAL work.
- No `<Style>` blocks inside view files (see Open item above for the one known exception)
- WPF views must not use WinUI-style layout properties such as `StackPanel.Spacing` or `Grid.ColumnSpacing`; use tokenized margins, padding, or spacer columns instead
- Receipt exception: `ReceiptWindow.xaml` and `ReceiptPrintView.xaml` may keep fixed numeric print layout values
- Old resource system is gone; do not use legacy keys/files

## Token state

- Token files are authored and live under `WPF/Themes/Tokens/`
- `Colors.xaml`, `Typography.xaml`, `Spacing.xaml`, and `Shadows.xaml` are active
- `Sizing.xaml` currently holds touch/icon sizes; radius keys are currently defined in `Spacing.xaml` and should be treated as the live source of truth unless intentionally reorganized
- **New tokens added 2026-08-16**: `Radius.IconBadge` (7px corner radius for icon badge borders), `Spacing.DialogErrorBannerMargin`, `Spacing.DialogListMargin`, `Spacing.DialogListHeaderGap`, `Spacing.DialogEditMargin`, `Spacing.DialogFieldGap` (dialog layout spacing tokens)
- **New tokens added 2026-08-21**: `Spacing.DialogSectionNextGap` (`0,16,0,4` — top margin for a second/non-first section header inside a dialog body column); `Spacing.KeyHintOverlayOffset` (`0,0,4,0` — right-side pull for a KeyHint badge overlaid top-right on a button)

## Skill routing

- Visual-layer rework: `skills/wpf-view-rebuild/SKILL.md`
- Styling/control/chrome work: `skills/wpf-styling/SKILL.md`
- Token work: `skills/wpf-token-system/SKILL.md`
- New non-CRUD screens: `skills/wpf-view-creation/SKILL.md`
- CRUD screens: `skills/wpf-crud-screen/SKILL.md`
- ViewModel work: `skills/wpf-mvvm-viewmodel/SKILL.md`
- Navigation: `skills/wpf-navigation/SKILL.md`
- Dialog behavior: `skills/wpf-dialogs/SKILL.md`
- Shared controls/converters/behaviors: `skills/wpf-reusable-controls/SKILL.md`
- Performance: `skills/wpf-performance/SKILL.md`

## Durable notes

- Historical runtime/build blockers and fixes were moved to `KownIssues.md`.
- Old `WPF/Resources/Common/*` dictionaries are retired; `WPF/Resources/Converters.xaml` is the only live file under `Resources/`.
- `WPF/App.xaml` uses the new token/control/chrome merge order only.
- `WPF_RESOURCE_COMPATIBILITY_CLEANUP_PLAN.md` tracks the completed short-key compatibility cleanup.
- Recent cashier and manager UX fixes to preserve are summarized in `KownIssues.md`.
- **Login/dashboard-switch and EF/DAL performance work (2026-08-16–17)**: see the grouped entry under Recent sessions below for the full set of changes (repository factory fix, lazy-load patterns, `Task.WhenAll` parallelization, EF pooled-factory standardization, stored-procedure login path, connection-pool tuning, startup warmup, sidebar idempotency).

## Recent sessions

- 2026-08-21: **Shortcut system overhaul + KeyHint control + ShortcutHelp dialog.**
  - `WPF/shortcuts.json`: Cashier keys updated to F3/F4/F5/F6 (Cash/Card/RecentSales/Reprint); dead keys `CompleteSale`, `FocusCategories`, `FocusProducts`, `FocusCart` removed. Manager gained `ModifierGroupManagement = "Alt+M"`.
  - `WPF/Configuration/ShortcutSettings.cs`: `CashierShortcuts` updated (removed 4 dead properties, added `ShowRecentSales` with fallback `"F5"`, updated `CashPayment`→`"F3"`, `CardPayment`→`"F4"`, `ReprintLastReceipt`→`"F6"`). `ManagerShortcuts` gained `ModifierGroupManagement` with fallback `"Alt+M"`.
  - `WPF/Behaviors/ShortcutBindingsBehavior.cs`: Cashier entries updated (removed 4 dead entries, added `ShowRecentSales → ShowRecentSalesCommand`). Manager entries gained `ModifierGroupManagement → NavigateModifierGroupManagementCommand`.
  - `WPF/Controls/KeyHint.xaml` + `KeyHint.xaml.cs`: new `UserControl`. `BadgeStyle` enum: `Default` (resolves `Color.Surface.Subtle`, `Color.Text.Muted`, `Color.Border.Default` via `FindResource`) | `OnBrand` (control-internal alpha constants `#28FFFFFF` bg, `#CCFFFFFF` fg, no border — justified exception to the no-hex-in-CS rule because these are design-owned overlays for brand-colored button surfaces). Frozen on static ctor.
  - `WPF/Views/Cashier/CashierDashboardView.xaml`: Cash and Card payment buttons now wrap content in a `Grid` with a `KeyHint` (`OnBrand`) overlay top-right. Reprint and RecentSales ghost buttons now contain a horizontal `StackPanel` with their text + `KeyHint` (`Default`).
  - `WPF/Views/Dialogs/ShortcutHelpView.xaml` + `.cs`: new dialog window. Shell matches `PaymentDialog`: `WindowStyle=None`, `AllowsTransparency`, `SizeToContent=WidthAndHeight`, `CenterOwner`, Width=560. Two-column body with `Common`+`Cashier` left, `Manager — Navigation`+`Manager — CRUD` right. No `<Style>` blocks, no hex.
  - `WPF/ViewModels/Dialogs/ShortcutHelpViewModel.cs`: new VM. `ShortcutRow(Label, Key)` record. Exposes `CommonRows`, `CashierRows`, `ManagerNavRows`, `ManagerCrudRows`. `CloseCommand` fires `DialogClosed` event. Registered as `AddTransient`.
  - `ShowShortcutHelpCommand` added to `CashierDashboardViewModel` and `ManagerMainViewModel`; both use `_viewModelFactory.Create<ShortcutHelpViewModel>()` + `_dialogService.ShowDialog<ShortcutHelpView>(vm)`.
  - `App.xaml.cs`: `services.AddTransient<ShortcutHelpViewModel>()` added next to other dialog VM registrations.

- 2026-08-16–17: **Auth/login performance pass.** Manager login opens shell before shift hydration; cashier login opens shell before shift hydration and refreshes dashboard after background lookup completes. `HomeViewModel` lazy-loads on first navigation via deferred `EnsureDataLoadedAsync()`; `CashierDashboardViewModel` same. `ReceiptManagementViewModel` moved off constructor eager-load. `HomeViewModel` dashboard sections parallelized with `Task.WhenAll`. `BCrypt.Verify` offloaded with `Task.Run`. `UserRepository.GetByUsernameAsync()` fixed: removed `Trim().ToLower()` from EF predicate so username lookup hits the DB index; raw ADO + `SP_LoginUser` stored proc used for manager-auth lookup. `RecentTransactionRepository`/`ShiftSummaryRepository` switched to `CreateDbContextAsync()`. Login warmup runs on startup. Connection pool set to `Min Pool Size=5`. Debug-only `TxpTrace` helpers added with `[TXP] -` prefix; cashier/manager per-section traces trimmed to keep only useful login/handoff timings.
- 2026-08-16–17: **EF optimization (Phases 1–4).** DAL standardized on `AddPooledDbContextFactory`; scoped `PosDbContext` and duplicate `ITransactionRepository` registrations removed; `Repository<T>` has one factory-only path. ADO-heavy repositories read connection string from `ISqlConnectionStringProvider` singleton. Default query tracking set to `NoTracking`; write flows use explicit `Add`/`Update`/`SaveChangesAsync`. `docs/EF_Optimization_Plan.md` is the source of truth. Build verified 0 errors/warnings via alternate output path.
- 2026-08-16–17: **Manager sidebar optimization.** Re-selecting the active page is now a no-op in `ManagerMainViewModel`. Sidebar navigation uses lazy-load entry points (`LoadAsync`) for transactions and shifts so revisits don't force a reload. Debug-only timing traces added for page-switch cost measurement.
- 2026-08-16–17: **UI/UX pass — manager and cashier surfaces.** Manager Home, Transactions, Reports, shared inputs redesigned with compact token-based layouts, 44px touch targets, horizontal-only padding, search/multiline/borderless input variants. Transaction rows fixed: no namespace text, plain numeric amounts, wider payment/status columns, wrapped notes, touch-friendly Void button. `ProductFormView` redesigned as two-column dialog with fixed header/footer and compact size/price table; `IsBusy`/`IsNotBusy` added to `ProductFormViewModel`. `ProductManagementView` got centered search-empty states with localized messages. `CategoryManagementView` reworked: root-only top strip, real `Id`/`ParentCategoryId` identity on cards, visible selection highlight, asset-backed category image, async busy-gated refresh. Modifier management redesigned as master-detail with overlay dialogs; `ModifierGroupManagementViewModel` restores selection after refresh. XAML parse failures fixed: `UnsetValue` border issues, hardened `POS.Dialog.CloseButton`/`POS.Button.Base`, removed invalid `WindowStartupLocation` style setter, corrected `Color.Surface.Page`/alpha ARGB/`Spacing.0`. Old `Resources/Common/*` retired; `App.xaml` on new token/control/chrome merge path. Cashier search/category/modifier/card/cart/footer fixes preserved. Durable rules added to `KownIssues.md` and `Memory.md`.
- 2026-08-19: **PIN overlay hardening.** Rewrote `ManagerPinOverlayView` from scratch following `PaymentDialog`/`StartDayDialog` shell pattern. PIN dot styles (`POS.Dialog.PinDot`, `.1`–`.4`), numpad key (`POS.Dialog.NumpadKey`), and settings badge/text (`POS.Settings.PinStatusBadge`/`POS.Settings.PinStatusText`) moved into `Themes/Controls/Dialogs.xaml`. Both views hold zero `<Style>` blocks and use only real tokens.
- 2026-08-20: **PIN System Step 6 — Void requires PIN approval.** Added `IManagerOverlayService managerOverlayService` constructor parameter to `TransactionsViewModel`; assigned `_managerOverlayService` in body. Replaced `VoidTransactionAsync` in `TransactionsViewModel.Actions.cs`: `MessageBox.Show` flow removed, PIN overlay (`RequestApprovalAsync` with `reasonRequired: true`) now acts as both confirmation and authorization. Removed orphaned `using System.Windows;`. No new DI registrations — both services already `AddTransient` in `App.xaml.cs`. Keys `Void.ApprovalTitle` and `Transactions.VoidFailed` confirmed in `en.xaml`. No build run per task instructions.
- 2026-08-20: **Void-no-PIN bug + no-PIN overlay UX.** `VoidTransactionAsync` voided even when approval failed (cancelled/wrong/locked PIN or **no PIN set**) because `RequestApprovalWithReasonAsync` returned only the reason string (null on failure) and the caller never checked approval. Fixed: `IManagerOverlayService.RequestApprovalWithReasonAsync` now returns `ManagerApprovalResult(bool Approved, string? Reason)`; the void caller aborts unless `Approved`. `ManagerPinOverlayViewModel` gained `IsPinSet`/`ShowPinNotice` + `InitializeAsync()` (checks `HasPinAsync`, cached post-login); `ManagerOverlayService` calls `InitializeAsync()` before showing in both methods. When no PIN, `ManagerPinOverlayView` shows a warning banner + centered "set PIN in Settings" (`ManagerPin.NoPinBanner`/`NoPinMessage` in en/ar/ml) and disables the numpad (`IsEnabled="{Binding IsPinSet}"`; Digit/Backspace/Confirm canExecute also gated). Added shared `POS.Dialog.NoticeWarning` to `Themes/Controls/Dialogs.xaml`. Build verified 0 errors/0 warnings via alternate output path.