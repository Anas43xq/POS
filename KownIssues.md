# KownIssues.md - WPF UI Rebuild

Compact record of known WPF rebuild issues, fixes, and rules learned from failures. Check this before debugging XAML runtime/build errors.

## Resource And Token Issues

- `Color.Surface.Page` is not a live token. Use `Color.Brand.Light` or another existing surface token until the token set is intentionally expanded.
- `Font.Weight.Regular` is not a live token. Use `Font.Weight.Normal` for normal-weight text.
- `Spacing.0` is required by active views and now exists in `Themes/Tokens/Spacing.xaml`.
- `ColumnDefinition.Width` expects a `GridLength`, not a `sys:Double` size token. For tokenized gaps inside grids, use `Width="Auto"` on the column and place an empty spacer element in that column with `Width="{StaticResource Size.*}"`.
- Inline alpha ARGB values in `Colors.xaml` caused `SolidColorBrush.Color` parse failures. Use RGB color values plus brush `Opacity` for translucent brushes.
- `DropShadowEffect.Color` and `GradientStop.Color` expect `Color` structs, not brushes. Use inline hex only in shadow/gradient definitions.
- Temporary short brush aliases such as `BrandDark`, `TextMuted`, `BadgeBaseBg`, `DisabledFg`, `ListRowBorder`, etc. were removed. Active files should use final `Color.*` keys only.
- `Color="#..."` belongs only in `Colors.xaml` and `Shadows.xaml`, except gradient stops that require `Color` structs inside chrome resources.

## Style And Template Issues

- A control template that changes a child `BorderBrush` in triggers must provide a real base `BorderBrush` and bind it through the template. Missing base values can restore `DependencyProperty.UnsetValue`.
- VM reload paths that cancel/replace a `CancellationTokenSource` must clear the stored field before disposing the active source; leaving `_loadCts` pointing at a disposed instance causes `ObjectDisposedException` on the next `_loadCts.Cancel()`.
- Single-line text inputs use fixed `Size.TouchTarget.Min`, horizontal-only padding, and a full-height `PART_ContentHost`; vertical padding in the content host makes login/edit fields look oversized and top-biased. Use `POS.TextBox.Multiline` for notes/descriptions, `POS.TextBox.Search` for standalone search, `POS.TextBox.SearchInline` inside icon search shells, and borderless textboxes inside `POS.Dialog.InputBorder`.
- `POS.Button.Base` carries safe transparent border defaults so derived buttons never begin with an unset border.
- `POS.Dialog.CloseButton` was hardened with explicit border values and template bindings.
- Overlay/window styles must set explicit `BorderBrush` and `BorderThickness`; relying on framework defaults caused `UnsetValue` parse failures.
- `WindowStartupLocation` is not a dependency property and cannot be set inside a `Window` style. Keep it as a local window attribute.
- Control dictionaries must not reference chrome-only aliases that merge later. Use tokens or already-loaded control resources.
- Views must not define local `<Style>` blocks. Move reusable variants into `Themes/Controls/*` or `Themes/Chrome/*`.
- WPF only allows one `Style` assignment per element. Do not combine `Style="..."` with nested `<TextBlock.Style>`.
- WPF does not support WinUI-style layout properties such as `StackPanel.Spacing` or `Grid.ColumnSpacing`. Use tokenized margins, padding, or a spacer row/column instead.
- `LoginWindow` needed an explicit `Click` handler on the Cancel button because `IsCancel="True"` alone did not dismiss this custom login shell reliably; closing the window in code-behind fixed the no-op cancel action.
- `RelayCommand` has an `(Action<object?>, Func<object?, bool>?)` overload and a separate `(Action, Func<bool>)` overload, but no cross overload. `new RelayCommand(_ => f(), () => g())` fails with `CS1593 Func<object?, bool> does not take 0 arguments` because the `_ =>` execute picks the `Action<object?>` overload and the `() =>` canExecute can't become `Func<object?, bool>`. Fix: give the canExecute a `_` parameter (`_ => !busy && ...`) so both lambdas are object-taking, or use the no-arg `Action` overload. `AsyncRelayCommand` (non-generic) takes a `Func<bool>` canExecute, so its `() =>` form is correct.

## Toast / Overlay Binding Issues

- A self-contained overlay that owns its `DataContext` (e.g. `ToastHost` binding `{Binding Toasts}`) must set that `DataContext` in its **constructor**, not in `Loaded`. Deferring to `Loaded` leaves the inherited window `DataContext` (`MainViewModel`) in force while bindings first evaluate → `BindingExpression` path error #40 (`'Toasts' property not found`). `App.ServiceProvider` is already built before any window is created, so the constructor is a safe resolution point.
- **Toast ransom on dropped keys:** `ToastTypeToBrushConverter` originally looked up bare resource keys (`SuccessDark`, `SuccessGreen`, `DangerRed`, `InfoBlue`, etc.) that existed nowhere in `Colors.xaml`. Every `TryFindResource` returned `null` → fell back to `Brushes.Transparent` for all four brushes (background, border, accent, text), so toasts rendered but were invisible — the toast flow appeared broken even though `_toasts.Count` incremented. Always resolve brush keys as `TryFindResource` against the real `Color.X.Y` tokens (`Color.Success.Default/.Dark/.Light/.Border`, `Color.Danger.*`, `Color.Warning.*`, `Color.Info.*`); a bare alias that returns `null` silently makes the UI transparent.
- **Toast must not sit over the header Logout.** The toast stack was anchored `Margin="0,16,16,0"` (top-right), which put the toast's ✕/copy buttons directly on top of the cashier/manager header's top-right **Logout** button. A click in that corner could hit Logout underneath instead of dismissing the toast → "pressing X logs me out". Fix: keep the stack top-right but set top margin to `64` so it clears the 52px header, and mark the dismiss click `e.Handled = true` in `ToastHost.OnDismissClick` so it can never fall through.
- Auto-dismiss is **enabled** (re-enabled after a period of being commented out): non-Error toasts auto-dismiss after 4s (`DisplayDuration`); Error toasts persist until dismissed via ✕ / `Dismiss`.

## List And DataGrid Issues

- `DataGridDetailsPresenter` in the default row template caused WPF to show object namespace/type text such as `Contracts.Transactions.TransactionListItemDto`. Default rows must not include a details presenter unless a real details template is provided.
- List, check-list, and DataGrid rows must bind explicit display fields/templates; never rely on object `ToString()`.
- Dense list/table row amounts intentionally show plain numeric values with no currency glyph or symbol.
- `ManagerDataGridColumnHeader.Right` exists for right-aligned header cells; use it instead of local header styles in views.
- Notes or long text columns should wrap through a template column when plain `DataGridTextColumn` clips.

## Currency Issues

- Use `controls:CurrencyText` for totals, payment panels, receipts, KPI summaries, and standalone money displays.
- Do not use `StringFormat={}{0:C}`, `ToString("C")`, inline currency symbols, or one-off currency layouts in views.
- Dense list/table row amounts use plain numeric formatting such as `StringFormat={}{0:N2}`.
- `CurrencyText.ShowIcon` exists; set it off where a clean amount-only display is needed.

## Cleanup History

- Old `WPF/Resources/Common/*` dictionaries and old chrome dictionaries were retired. `WPF/Resources/Converters.xaml` is the only remaining live file under `WPF/Resources/`.
- `WPF/App.xaml` no longer merges old resource files such as `Resources/Common/Brushes.xaml`, `Buttons.xaml`, `DataGrid.xaml`, `Lists.xaml`, `FilterField.xaml`, or old dashboard/dialog chrome files.
- Shared manager filter/card/KPI chrome moved into `Themes/Chrome/AppChrome.xaml`.
- Active control/view references were migrated from old aliases to `POS.*`, `Color.*`, and final token keys.
- `POS.Button.Outline` is a live style key and must remain; only old aliases like `OutlineBtn` are retired.

## UX Fixes To Preserve

- Cashier product search filters localized and English names and is debounced.
- Cashier add-to-cart skips modifier lookup for products without modifier groups.
- Cashier category selection uses a single radio-style active state and product lists scroll to top after category/subcategory changes.
- Cashier footer payment actions were widened so cash/card totals do not clip.
- Cashier product cards, cart rows, and modifier option buttons were adjusted to wrap long names without clipping.
- Manager Home list typography is compact; recent transaction receipt numbers are bordered open buttons.
- Transaction list receipt button, payment/status widths, wrapped notes, and toolbar Void button sizing were tuned for touch, mouse, and keyboard use.
