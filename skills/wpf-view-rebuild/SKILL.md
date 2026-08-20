---
name: wpf-view-rebuild
description: Defines the exact process for rebuilding a single WPF view file - replacing all hardcoded values with token references, applying the correct POS-namespaced control styles, and verifying the result. Use for every view touched during Phase 3 (Cashier), Phase 4 (Manager), and Phase 5 (Shared/Auth). Not for authoring token files or control styles - see wpf-token-system and wpf-styling for those.
---

# WPF View Rebuild

## When to use this

Rebuilding any of the 49 view files. A "rebuilt" view means: zero raw values, zero legacy keys, every element using a token reference or a POS-namespaced style. Every view must reach this state before the rebuild is considered done.

---

## Before you start a view

Read these first, every time. Don't assume you remember the rules:

1. `skills/wpf-styling/SKILL.md` - what's allowed, what's not, verification checklist
2. `skills/wpf-token-system/SKILL.md` - token keys and type rules
3. The view's corresponding ViewModel to understand what bindings exist - you're rebuilding the visual layer only; bindings must survive unchanged

Never delete a `{Binding}` expression. Never rename a binding path. Never change a `Command=` binding. The ViewModel is untouched. If a binding breaks during rebuild, you made a structural mistake - fix it.

---

## The rebuild process, step by step

### Step 1 - Read the existing view completely

Before writing a single line, understand what's there. Note:
- What layout containers exist (`Grid`, `StackPanel`, `DockPanel`, etc.)
- What bindings exist - every `{Binding ...}` must be preserved exactly
- What's hardcoded (colors, margins, fonts) - these will all be replaced
- What legacy style keys are used - these will all be replaced

Don't start editing until you've read the full file.

### Step 2 - Identify every hardcoded value

Scan for these patterns - these are everything that needs to change:

```
Foreground="#..."              -> Color.Text.Default or appropriate token
Foreground="White"             -> Color.Text.OnBrand (if on brand bg) or appropriate token
Background="#..."              -> appropriate Color.Surface.X or Color.Brand.X token
BorderBrush="#..."             -> appropriate Color.Border.X token
Margin="8,0,0,0"               -> Spacing.2 or closest token
Padding="12,8"                 -> closest Spacing token (Spacing.3 vertical, Spacing.2 horizontal)
FontSize="14"                  -> Font.Size.Base
FontSize="12"                  -> Font.Size.Sm
FontFamily="..."               -> Font.Family.Default
CornerRadius="8"               -> Radius.Default
Width="44" / Height="44"       -> Size.TouchTarget.Min
Style="{StaticResource BrandDark}"            -> old key, gone
Style="{StaticResource PrimaryGradientBtn}"    -> old key -> POS.Button.Primary
Style="{StaticResource OutlineBtn}"            -> old key -> POS.Button.Secondary
Style="{StaticResource DangerBtn}"             -> old key -> POS.Button.Danger
Text="{Binding Amount, StringFormat={}{0:C}}"  -> replace with `CurrencyText` for totals/standalone money, or plain `{0:N2}` for list/table row amounts
```

Write these down or comment them before editing. Know all the changes before making any.

### Step 3 - Rebuild the structure

Keep the layout structure (Grid row/column definitions, StackPanel orientation, DockPanel dock) - this is visual organization, not a value, and often represents real intent. Replace all visual values with tokens:

**Colors:**
```xml
<!-- Before -->
<Border Background="#2589E0" BorderBrush="#1A6FC4">
<!-- After -->
<Border Background="{StaticResource Color.Brand.Primary}"
        BorderBrush="{StaticResource Color.Brand.Dark}">
```

**Text:**
```xml
<!-- Before -->
<TextBlock Foreground="#1A2332" FontSize="14" FontFamily="Lato">
<!-- After -->
<TextBlock Foreground="{StaticResource Color.Text.Default}"
           FontSize="{StaticResource Font.Size.Base}"
           FontFamily="{StaticResource Font.Family.Default}">
```

**Spacing:**
```xml
<!-- Before -->
<StackPanel Margin="16,8,16,8">
<!-- After -->
<StackPanel Margin="{StaticResource Spacing.4}">
<!-- Note: if margins are asymmetric, use the closest matching Thickness token,
     or restructure the layout to eliminate the need for asymmetric margins -->
```

**Buttons:**
```xml
<!-- Before -->
<Button Content="Save" Style="{StaticResource PrimaryGradientBtn}"/>
<!-- After -->
<Button Content="Save" Style="{StaticResource POS.Button.Primary}"/>
```

**Money display:**
```xml
<!-- Before -->
<TextBlock Text="{Binding Total, StringFormat={}{0:C}}"/>
<!-- After -->
<controls:CurrencyText Amount="{Binding Total}"
                       AmountForeground="{StaticResource Color.Text.Default}"/>
```

**Money in list/table rows:**
```xml
<!-- Dense list rows show amount only, without currency glyph/symbol -->
<TextBlock Text="{Binding GrandTotal, StringFormat={}{0:N2}}"/>
```

**Touch targets:**
```xml
<!-- Every interactive element gets this - no exceptions -->
<Button MinHeight="{StaticResource Size.TouchTarget.Min}" .../>
<ComboBox MinHeight="{StaticResource Size.TouchTarget.Min}" .../>
<TextBox MinHeight="{StaticResource Size.TouchTarget.Min}" .../>
```

**Foreground - never on the element:**
```xml
<!-- Before (wrong - blocks trigger) -->
<Button Content="Save" Foreground="White" Style="{StaticResource POS.Button.Primary}"/>

<!-- After (right - style setter owns Foreground) -->
<Button Content="Save" Style="{StaticResource POS.Button.Primary}"/>
```

### Step 4 - Remove all inline styles

No `<Style>` blocks inside a view file. No `<ResourceDictionary>` in `<UserControl.Resources>` or `<Window.Resources>` that defines styles or brushes. Resource dictionaries in a view are only allowed for converters that are genuinely used in that view and nowhere else - and even then, those converters belong in `Resources/Converters.xaml`, registered globally.

If you find a style defined inside the view that doesn't exist as a global POS-namespaced key, one of two things is true:
1. A matching global style exists and the old inline style was a legacy override - remove the inline style and use the global key.
2. A matching global style does not exist - it needs to be added to the correct `Themes/Controls/` file first, then referenced here.

Never leave an inline style in place because "it's easier." That's how the old system happened.

### Step 5 - Verify bindings survived

After rebuilding, compare the binding list you noted in Step 1 against the rebuilt file. Every `{Binding ...}` must still be present with the same path, same mode, same converter. A missing binding is a functional regression, not a style issue.

### List row display rule

List, check-list, and DataGrid rows must bind to explicit display fields or templates. Never let a row content presenter/details presenter fall back to the bound object itself, because WPF will display namespace/type text such as `Contracts.Transactions.TransactionListItemDto`.

---

## Handling asymmetric spacing

The token scale uses uniform `Thickness` values (all four sides equal). Real layouts often need asymmetric margins (e.g. `8,0,0,0` for left-only margin). Options, in order of preference:

1. **Restructure the layout** - often a margin of `8,0,0,0` exists to create separation between siblings; a `StackPanel` with `Spacing` property (WPF 4.6+) or a `Grid` with column spacing eliminates the need.
2. **Use the closest token** - if the asymmetric margin is `8,0,0,0`, use `Spacing.2` on the element and accept that it applies to all sides, then adjust the layout container to compensate.
3. **Explicit `Thickness` as a local resource** - if an asymmetric value genuinely can't be avoided, define it as a `<Thickness x:Key="...">8,0,0,0</Thickness>` in `Spacing.xaml` with a descriptive key (e.g. `Spacing.LeftOnly.2`). Don't inline the literal in the view.

Option 3 is a last resort, not a default. If you find yourself adding many asymmetric spacing tokens, the layout needs restructuring.

---

## Cashier surface rebuild (Phase 3)

Cashier views have additional constraints:

- **All interactive elements: `MinHeight="{StaticResource Size.TouchTarget.Min}"`** - 44px minimum, no exceptions. This surface is used on touch screens.
- **Product grid buttons** - large tap targets. Minimum `Height="{StaticResource Size.TouchTarget.Min}"`, prefer larger.
- **Cart items** - swipe/tap-friendly row height. 44px minimum per row.
- **No hover-only states** as the only feedback - every interactive element must have a Pressed state visible to touch, since hover doesn't exist on touch.
- **Money values** - use `controls:CurrencyText` for totals, prices, and payment amounts. In dense list/table rows, show plain numeric amounts with `{0:N2}` and no currency glyph/symbol. Use `AmountForeground` and `IconSize` for appearance where `CurrencyText` is appropriate; do not use `StringFormat={}{0:C}` or other culture-based currency formatting in the view.
- **Icon glyphs** - if you keep a decorative glyph in a view, use a valid Unicode character or an image asset. Remove corrupted mojibake rather than shipping it. If the glyph is only a placeholder for a future image icon, keep the placeholder in place and replace it later.
- **Receipt surfaces** - `ReceiptWindow.xaml` and `ReceiptPrintView.xaml` are print outputs. Fixed numeric layout values are allowed there when they are part of the receipt format.

Rebuild order within the cashier surface (Priority 1 first):
1. `CashierDashboardView` - layout shell, no bindings of its own, structure only
2. `CashierHeaderView`
3. `ProductsPanelView`
4. `CartPanelView`
5. `ModifierPanelView`
6. `PaymentDialog` / `CardPaymentConfirmDialog`
7. Priority 2: `QuickActionsPanelView`, `ErrorBannerView`, `ReceiptWindow`, `ReceiptPrintView`, `RecentTransactionView`, `RecentSalesDialog`

### Done when: full cashier flow works - open shift -> select products -> add to cart -> apply modifier -> pay -> receipt.

---

## Manager surface rebuild (Phase 4)

Manager views are data-heavy. Additional considerations:

- **`DataGrid` must use `Style="{StaticResource POS.DataGrid.Default}"`** - no per-screen grid styling.
- **Filter panels** - use `POS.TextBox.Default` and `POS.ComboBox.Default` for all filter inputs. The `FilterField.xaml`-based `FieldLabel` style is gone; use `Foreground="{StaticResource Color.Text.Muted}"` and `FontSize="{StaticResource Font.Size.Sm}"` on labels directly, or extract a `POS.Label.Field` style into `Themes/Controls/Inputs.xaml` if you find yourself repeating this pattern across multiple filter panels.
- **`CrudActionButtons` control** - still used as-is (it's a UserControl, not a styled element). No changes to its internal structure.
- **Dashboard cards** - the `DashboardCardBorder` key is gone. Use `Border` with `Background="{StaticResource Color.Surface.Default}"`, `CornerRadius="{StaticResource Radius.Default}"`, `Effect="{StaticResource Shadow.Default}"` directly.

Rebuild order within the manager surface (Priority 1 first):
1. `ManagerMainView` - shell and nav sidebar
2. `HomeView` + `KpiMetricsControl`
3. `TransactionsView`
4. `ReportView`
5. `ProductManagementView` + `ProductFormView`
6. `ShiftManagementView` + `StartDayDialog` + `EndDayDialog`
7. Priority 2: all remaining management screens

### Done when: full manager flow works - dashboard -> products -> categories -> reports -> shifts -> settings.

---

## Shared / Auth rebuild (Phase 5)

- `LoginAsWindow` - auth surface. Clean, brand-forward. Uses `Color.Brand.Primary` as primary surface.
- `ManagerLoginDialog` - use `Themes/Controls/Dialogs.xaml` shell.
- `TranslationDialogView` - use dialog shell chrome from `Dialogs.xaml`.

---

## Verification - run after every view

Run this on every file before marking it done. Zero tolerance - one hit is a failure:

```
grep '="#'                 in the file -> 0 results  (no inline hex)
grep 'Foreground="'        in the file -> 0 results
grep 'Background="'        in the file -> 0 results
grep 'BorderBrush="'       in the file -> 0 results
grep 'FontSize="'          in the file -> 0 results
grep 'FontFamily="'        in the file -> 0 results
grep 'Margin="[0-9]'       in the file -> 0 results  (literal margin value)
grep 'Padding="[0-9]'      in the file -> 0 results
grep 'CornerRadius="'      in the file -> 0 results  (literal radius)
grep 'Color="{StaticResource' -> 0 results           (type mismatch)
grep 'StringFormat={}{0:C}\|ToString("C")' in the file -> 0 results (no culture-based currency formatting)
grep 'StaticResource BrandDark\|PrimaryGradientBtn\|OutlineBtn\|DangerBtn\|ModalCardBorder\|DashboardCardBorder\|FieldLabel' -> 0 results (old keys)
```

Then:
- [ ] App starts without `XamlParseException`
- [ ] No missing resource key warnings in output window
- [ ] All bindings from the original file are still present
- [ ] Every `Button` has `Style="{StaticResource POS.Button.X}"`
- [ ] Every interactive element has `MinHeight="{StaticResource Size.TouchTarget.Min}"`
- [ ] Text on brand-colored backgrounds uses `Foreground="{StaticResource Color.Text.OnBrand}"`
- [ ] Hover, Pressed, Disabled states are visible (not relying on inline values that were removed)

---

## Common mistakes

**Missing resource key at runtime** - you referenced a key that doesn't exist yet in the token files. Check spelling exactly; keys are case-sensitive. Check that the token file is merged before the file referencing it in `App.xaml`.

**`XamlParseException` on `Color="{StaticResource ...}"`** - you used a brush key where a `Color` struct is expected. See the critical type rule in `wpf-token-system`. Use inline hex.

**Binding stopped working after rebuild** - you removed or changed a binding path while replacing surrounding markup. Restore it exactly.

**Hover state invisible** - a local `Background` or `Foreground` on the element is blocking the trigger in the control template. Remove all local visual-value attributes from the element; let the style own them.

**Touch target below 44px** - you set a fixed `Height` smaller than `Size.TouchTarget.Min`, or the default WPF button height is rendering below 44px. Add `MinHeight="{StaticResource Size.TouchTarget.Min}"`.

**Money still shows culture formatting** - replace totals/standalone money with `controls:CurrencyText` and bind `Amount`, `AmountForeground`, and `IconSize`; in dense list/table rows use plain numeric `{0:N2}` without a currency glyph or symbol.

---

## Don't

- Don't change ViewModel bindings - visual layer only.
- Don't define styles inside a view file under any circumstances.
- Don't leave a single literal color, spacing, font, or size value in the file.
- Don't use old key names from the previous resource system.
- Don't rebuild a view without running the verification grep list.
- Don't move to the next view until the current one passes all verification checks.
- Don't render money with culture-based currency formatting or inline currency symbols in a view. Dense list/table row amounts intentionally omit the currency glyph/symbol.

---

## Related files

`skills/wpf-styling/SKILL.md` - hard rules and control style keys
`skills/wpf-token-system/SKILL.md` - token key reference and type rules
`skills/wpf-mvvm-viewmodel/SKILL.md` - binding discipline (don't touch bindings)
`AGENTS.md` - rebuild phase plan, surface-level completion criteria

