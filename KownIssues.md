# KownIssues.md - WPF UI Rebuild

Compact record of WPF rebuild issues/rules. Check before debugging XAML runtime/build errors.

## Resource And Token Issues

- `Color.Surface.Page` and `Font.Weight.Regular` are not live tokens. Use `Color.Brand.Light` and `Font.Weight.Normal`.
- `Spacing.0` exists in `Themes/Tokens/Spacing.xaml` and is required by active views.
- `ColumnDefinition.Width` needs a `GridLength`, not a `sys:Double`. For grid gaps: `Width="Auto"` column + empty spacer sized by `Size.*`.
- Translucent brushes: RGB hex + `Opacity`, never inline alpha ARGB (parse failure).
- `DropShadowEffect.Color`/`GradientStop.Color` want `Color` structs — inline hex only in shadow/gradient defs, never a brush reference.
- `Color="#..."` is allowed only in `Colors.xaml` / `Shadows.xaml` (plus `Color`-struct gradient stops). Use final `Color.*` keys everywhere; short aliases (`BrandDark`, `TextMuted`, `BadgeBaseBg`, ...) are removed.

## Style And Template Issues

- Trigger that swaps a child `BorderBrush` must bind through a real base `BorderBrush`, or you restore `DependencyProperty.UnsetValue`.
- Reload paths: clear the stored `_loadCts` field before disposing/replacing it to avoid `ObjectDisposedException` on next `Cancel()`.
- Text inputs: single-line = fixed `Size.TouchTarget.Min`, horizontal-only padding, full-height `PART_ContentHost` (vertical padding looks oversized/top-biased). Use `POS.TextBox.Multiline` (notes), `.Search` (standalone), `.SearchInline` (icon shells), borderless inside `POS.Dialog.InputBorder`.
- `POS.Button.Base` and `POS.Dialog.CloseButton` carry explicit border defaults (avoid `UnsetValue`). Overlay/window styles must set explicit `BorderBrush`+`BorderThickness`.
- `WindowStartupLocation` isn't a DP — set locally, not in a `Window` style.
- Control dictionaries must not reference chrome-only aliases merged later. No local `<Style>` blocks in views; one `Style` per element; no `StackPanel.Spacing`/`Grid.ColumnSpacing` (use margins/padding/spacers).
- `LoginWindow` Cancel needs an explicit `Click` handler — `IsCancel="True"` alone no-ops on the custom shell.
- `RelayCommand` overloads don't cross: `(Action,Func<bool>)` vs `(Action<object?>,Func<object?,bool>?)`. For object-taking executers give canExecute a `_` param; non-generic `AsyncRelayCommand` takes `Func<bool>`.

## Toast / Overlay Binding Issues

- A self-contained overlay owning its `DataContext` (e.g. `ToastHost` on `{Binding Toasts}`) must set it in the **constructor**, not `Loaded` — otherwise the first binding pass sees the inherited window VM (`MainViewModel`, no `Toasts`) → `BindingExpression` error #40. `App.ServiceProvider` is ready before any window exists, so the ctor is the safe point.
- Toast converter must resolve real `Color.X.Y` keys (`Color.Success/Danger/Warning/Info.Default/.Dark/.Light/.Border`). Bare aliases (`SuccessDark`, `InfoBlue`, ...) return `null` → `Brushes.Transparent` → toast renders but is invisible.
- Toast stack must clear the 52px header: top-right `Margin="0,64,16,0"` (was `0,16,16,0`, sitting over the header Logout → "X logs me out"); `OnDismissClick` sets `e.Handled = true` so clicks never fall through.
- Auto-dismiss **enabled**: non-Error toasts dismiss after 4s; Error toasts persist until ✕ / `Dismiss`.
- Toast logging = lifecycle only: two `[TOAST]` lines in `NotificationService.Show` (show + persist-or-dismiss) and one in `ToastHost.OnDismissClick`. Don't reuse `[TOAST]` for payment-flow logging.

## List And DataGrid

- No `DataGridDetailsPresenter` in the default row template (shows namespace text). Bind explicit display fields/templates, never `ToString()`.
- Dense list/table row amounts: plain numeric (`N2`), no currency glyph/symbol.
- Use `ManagerDataGridColumnHeader.Right` for right-aligned headers; wrap long text via a template column when `DataGridTextColumn` clips.

## Currency

- Totals/payments/receipts/KPIs/money displays use `controls:CurrencyText`. No `{0:C}`, `ToString("C")`, inline symbols, or one-off layouts. Rows use `{0:N2}`. `CurrencyText.ShowIcon` off for amount-only.

## Cleanup History

- Old `WPF/Resources/Common/*`, dashboard/dialog chrome, and `Resources/Common/Brushes|Buttons|DataGrid|Lists|FilterField` are retired. `Resources/Converters.xaml` is the only live file under `WPF/Resources/`. `App.xaml` merges fixed token→utilities→controls→chrome order only.
- Shared manager filter/card/KPI chrome lives in `Themes/Chrome/AppChrome.xaml`. Aliases migrated to `POS.*` / `Color.*`. `POS.Button.Outline` stays; `OutlineBtn` etc. are gone.

## UX Fixes To Preserve

- Cashier: search filters localized+English names and is debounced; add-to-cart skips modifier lookup when no group; single radio-style category state + scroll-to-top; wide footer payment actions; long names wrap in product cards/cart rows/modifier options.
- Manager: compact Home typography, bordered open-button receipt numbers; transaction list receipt/payment/status/notes widths and toolbar Void sized for touch/mouse/keyboard.