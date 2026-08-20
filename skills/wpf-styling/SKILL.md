---
name: wpf-styling
description: Defines the WPF design-system resource layer for the rebuilt POS UI â€” five token files under Themes/Tokens/, control styles under Themes/Controls/, chrome files under Themes/Chrome/ â€” and the hard rules that make the system work. Use whenever writing or editing any XAML that touches visual values (colors, spacing, fonts, sizes, shadows, control templates, chrome). This skill is the visual-layer law. The old Brushes.xaml / DashboardChrome.xaml / FilterField.xaml system is gone â€” don't reference it.
---

# WPF Styling

## When to use this

Any time you write or edit XAML that contains a visual value â€” color, spacing, font, size, radius, shadow â€” or any control style, template, or chrome layout. Read this before touching any file under `Themes/`. Read this before touching any view file. The rules here are not guidelines; violation is a bug.

---

## The resource layer

Merged in `App.xaml` in this fixed order. Comments in `App.xaml` are mandatory â€” the order is load-order-dependent and not obvious:

```xml
<!-- â‘  Tokens â€” no dependencies, must load first -->
<ResourceDictionary Source="Themes/Tokens/Colors.xaml"/>
<ResourceDictionary Source="Themes/Tokens/Typography.xaml"/>
<ResourceDictionary Source="Themes/Tokens/Spacing.xaml"/>
<ResourceDictionary Source="Themes/Tokens/Sizing.xaml"/>
<ResourceDictionary Source="Themes/Tokens/Shadows.xaml"/>

<!-- â‘¡ Utilities â€” depend on tokens only -->
<ResourceDictionary Source="Resources/Converters.xaml"/>

<!-- â‘¢ Controls â€” depend on tokens only -->
<ResourceDictionary Source="Themes/Controls/Buttons.xaml"/>
<ResourceDictionary Source="Themes/Controls/Inputs.xaml"/>
<ResourceDictionary Source="Themes/Controls/DataGrid.xaml"/>
<ResourceDictionary Source="Themes/Controls/Lists.xaml"/>
<ResourceDictionary Source="Themes/Controls/CheckRadio.xaml"/>
<ResourceDictionary Source="Themes/Controls/Dialogs.xaml"/>

<!-- â‘£ Chrome â€” depend on tokens + controls -->
<ResourceDictionary Source="Themes/Chrome/AppChrome.xaml"/>
<ResourceDictionary Source="Themes/Chrome/CashierChrome.xaml"/>
```

Do not change this order. Do not add a new file without placing it in the correct tier.

---

## Token files â€” what lives where

### `Themes/Tokens/Colors.xaml`

- Contains `SolidColorBrush` resources only.
- Every entry is a brush with an inline hex value. Nothing else.
- Keys follow the pattern `Color.<Category>.<Role>` â€” e.g. `Color.Brand.Primary`, `Color.Text.Default`, `Color.Surface.Page`.
- **Never** use `Color="{StaticResource ...}"` on a `SolidColorBrush` â€” `Color` expects a `Color` struct; `StaticResource` from this file is a `SolidColorBrush`. Type mismatch. Silent XAML failure. See the SolidColorBrush rule below.

### `Themes/Tokens/Typography.xaml`

- Contains `FontFamily` resources, `sys:Double` font-size resources, and `FontWeight` struct resources.
- Keys: `Font.Family.Default`, `Font.Size.Base`, `Font.Weight.SemiBold`, etc.
- Font sizes are `sys:Double`, not `x:Static` â€” they bind to `FontSize="{StaticResource Font.Size.Base}"` directly.

### `Themes/Tokens/Spacing.xaml`

- Contains `Thickness` resources on a 4px base scale.
- Keys: `Spacing.1` (4px), `Spacing.2` (8px), `Spacing.3` (12px), `Spacing.4` (16px), `Spacing.5` (20px), `Spacing.6` (24px), `Spacing.8` (32px), `Spacing.10` (40px), `Spacing.12` (48px).
- Used as `Margin="{StaticResource Spacing.4}"` or `Padding="{StaticResource Spacing.2}"`.

### `Themes/Tokens/Sizing.xaml`

- Contains `sys:Double` resources for heights, widths, icon sizes, touch targets, and corner radii.
- Keys: `Size.TouchTarget.Min` (44px), `Size.Icon.Default` (20px), `Radius.Default` (8px), `Radius.Xl` (16px), etc.
- Every interactive element must have a `MinHeight="{StaticResource Size.TouchTarget.Min}"` or `Height="{StaticResource Size.TouchTarget.Min}"`. No exceptions.

### `Themes/Tokens/Shadows.xaml`

- Contains `DropShadowEffect` resources with **inline hex values only**.
- Keys: `Shadow.Sm`, `Shadow.Default`, `Shadow.Lg`, `Shadow.None`.
- **Never** use `Color="{StaticResource ...}"` on a `DropShadowEffect` â€” `DropShadowEffect.Color` expects a `Color` struct, not a `SolidColorBrush`. Inline hex only.

---

## Control files â€” what they contain

### `Themes/Controls/Buttons.xaml`

All button styles and `ControlTemplate`s. Variants: `POS.Button.Primary`, `POS.Button.Secondary`, `POS.Button.Danger`, `POS.Button.Ghost`. Each variant's template covers: Default, Hover, Pressed, Disabled, Focused states â€” all states, no exceptions. Foreground is owned by the style setter only â€” never set on the button element in a view, never set on an inner `TextBlock`.

### `Themes/Controls/Inputs.xaml`

`TextBox`, `ComboBox`, `PasswordBox`, `DatePicker`. Style key pattern: `POS.TextBox.Default`, `POS.ComboBox.Default`. Every input style must cover Default, Focused, Disabled, Error states.

### `Themes/Controls/DataGrid.xaml`

`DataGrid` style plus row, header, and cell templates. Key: `POS.DataGrid.Default`. Row hover and selection states are covered inside the template â€” not set per-screen.

### `Themes/Controls/Lists.xaml`

`ListBox` and `ListBoxItem` styles. Badges, chips. Keys: `POS.ListBox.Default`, `POS.Badge.Default`, etc.

### `Themes/Controls/CheckRadio.xaml`

`CheckBox`, `RadioButton`, `ToggleButton` styles and templates.

### `Themes/Controls/Dialogs.xaml`

Dialog shell chrome â€” the outer `Border` shape (shadow, corner radius, background) that every dialog window sits inside.

---

## Chrome files

### `Themes/Chrome/AppChrome.xaml`

Manager surface navigation sidebar, header, and layout shell. Not per-screen â€” defines the structural chrome that all manager views share.

### `Themes/Chrome/CashierChrome.xaml`

Cashier surface layout shell and header. Same principle.

---

## Hard rules â€” violation is a bug, not a style preference

### Never in any view or control file

```
Foreground="..."                               â†’ must be in a style setter
Background="..."                               â†’ must be in a style setter
BorderBrush="..."                              â†’ must be in a style setter
FontSize="..."                                 â†’ must be a token: {StaticResource Font.Size.Base}
FontFamily="..."                               â†’ must be a token: {StaticResource Font.Family.Default}
Margin="8,0,0,0"                              â†’ must be a token: {StaticResource Spacing.2}
Padding="12,8"                                â†’ must be a token
CornerRadius="8"                              â†’ must be a token: {StaticResource Radius.Default}
Color="..."                                   â†’ only inside Colors.xaml or Shadows.xaml, nowhere else
<SolidColorBrush Color="{StaticResource ...}" â†’ type mismatch, always wrong
DropShadowEffect Color="{StaticResource ..."  â†’ type mismatch, always wrong
<Style> defined in App.xaml                   â†’ not allowed
<Style> defined in a view file                â†’ not allowed
```

### Always required in views

```
Every Button          â†’ Style="{StaticResource POS.Button.X}"
Every TextBox         â†’ Style="{StaticResource POS.TextBox.Default}"
Every ComboBox        â†’ Style="{StaticResource POS.ComboBox.Default}"
Every DataGrid        â†’ Style="{StaticResource POS.DataGrid.Default}"
Every interactive el. â†’ MinHeight="{StaticResource Size.TouchTarget.Min}"
Foreground on button  â†’ set in style setter only, never on the element or inner TextBlock
```

### The SolidColorBrush rule â€” the root cause of the old system's bugs

`Color` (property type: `System.Windows.Media.Color` struct) and `SolidColorBrush` (type: `System.Windows.Media.SolidColorBrush`) are different types. WPF's resource system does not convert between them. When you set `Background="{StaticResource Color.Brand.Primary}"`, WPF receives a `SolidColorBrush` â€” correct. When you set `<SolidColorBrush Color="{StaticResource Color.Brand.Primary}"/>`, WPF receives a `SolidColorBrush` where it expected a `Color` struct â€” it silently ignores the binding or crashes at runtime.

```xml
<!-- WRONG â€” Color property expects struct, gets brush -->
<SolidColorBrush x:Key="Alias" Color="{StaticResource Color.Brand.Primary}"/>

<!-- WRONG â€” DropShadowEffect.Color expects struct, gets brush -->
<DropShadowEffect Color="{StaticResource Shadow.Default}"/>

<!-- RIGHT â€” brush with inline hex, only in Colors.xaml -->
<SolidColorBrush x:Key="Color.Brand.Primary" Color="#2589E0"/>

<!-- RIGHT â€” DropShadowEffect with inline hex, only in Shadows.xaml -->
<DropShadowEffect Color="#0F2A4A" Opacity="0.16" BlurRadius="24"/>

<!-- RIGHT â€” Background references the brush key -->
<Border Background="{StaticResource Color.Brand.Primary}"/>
```

### The Foreground ownership rule

A local value on an element always wins over a style setter or trigger. If you set `Foreground="White"` on a button in a view, the Disabled trigger in the control template â€” which tries to change `Foreground` to the disabled color â€” silently loses. The element stays white when disabled, with no error.

```xml
<!-- WRONG â€” local value blocks all triggers -->
<Button Foreground="White" Style="{StaticResource POS.Button.Primary}"/>

<!-- WRONG â€” inner TextBlock local value blocks template trigger -->
<Button Style="{StaticResource POS.Button.Primary}">
    <TextBlock Text="Save" Foreground="White"/>
</Button>

<!-- RIGHT â€” style setter owns Foreground, template triggers override on state -->
<Button Style="{StaticResource POS.Button.Primary}" Content="Save"/>
```

### The icon glyph rule

If a view uses a decorative glyph, it must be a valid Unicode character or a proper image asset. Do not leave mojibake in the file. If the symbol is only a placeholder for a future image icon, keep the placeholder in place and replace it later.

---

## Verification checklist â€” run after every file you touch

- [ ] `grep -r '="#'` in the file â†’ 0 results (no inline hex)
- [ ] `grep 'Foreground="'` in the file â†’ 0 results
- [ ] `grep 'Background="'` in the file â†’ 0 results
- [ ] `grep 'FontSize="'` in the file â†’ 0 results
- [ ] `grep 'Margin="'` with a literal value â†’ 0 results
- [ ] `grep 'Color="{StaticResource'` â†’ 0 results (type mismatch)
- [ ] Every `Button` has `Style="{StaticResource POS.Button.X}"`
- [ ] Every interactive element has `MinHeight="{StaticResource Size.TouchTarget.Min}"`
- [ ] App starts without `XamlParseException`
- [ ] No missing resource key warnings in output window

---

## What no longer exists â€” don't reference these

The following files from the old system are gone. Any reference to them is wrong:

```
Resources/Common/Brushes.xaml         â†’ replaced by Themes/Tokens/Colors.xaml
Resources/Common/Buttons.xaml         â†’ replaced by Themes/Controls/Buttons.xaml
Resources/Common/DataGrid.xaml        â†’ replaced by Themes/Controls/DataGrid.xaml
Resources/Common/Lists.xaml           â†’ replaced by Themes/Controls/Lists.xaml
Resources/Common/FilterField.xaml     â†’ replaced by Themes/Controls/Inputs.xaml
Resources/DashboardChrome.xaml        â†’ replaced by Themes/Chrome/AppChrome.xaml
Resources/DialogChrome.xaml           â†’ replaced by Themes/Controls/Dialogs.xaml
Resources/CashierDashboardStyles.xaml â†’ replaced by Themes/Chrome/CashierChrome.xaml
Resources/AppChrome.xaml              â†’ replaced by Themes/Chrome/AppChrome.xaml
```

Old key names (`BrandDark`, `BrandMid`, `PrimaryGradientBtn`, `ModalCardBorder`, `DashboardCardBorder`, `FieldLabel`) are gone. New keys use the `POS.` or `Color.` or `Font.` etc. namespace pattern.

---

## Related files

`skills/wpf-token-system/SKILL.md` â€” token file authoring, key naming, type rules  
`skills/wpf-view-rebuild/SKILL.md` â€” migrating an existing view to use the new token system  
`skills/wpf-reusable-controls/SKILL.md` â€” UserControls/converters that sit on top of these resources  
`AGENTS.md` â€” rebuild phase plan, hard rules, verification checklist

