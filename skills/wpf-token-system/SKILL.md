---
name: wpf-token-system
description: Defines how to author, extend, and correctly reference the five token files (Colors, Typography, Spacing, Sizing, Shadows) that are the single source of truth for every visual value in the WPF POS UI. Covers key naming, type rules, what each file may and may not contain, and the XAML type traps that caused the original system's silent failures. Use whenever writing or editing any token file, adding a new token, or debugging a resource-not-found or type-mismatch error.
---

# WPF Token System

## When to use this

Authoring or editing `Themes/Tokens/*.xaml`. Adding a new color, spacing value, size, or shadow. Debugging a `XamlParseException`, a missing resource key, or a visual that looks wrong at a specific state. Understanding why a certain pattern is forbidden.

---

## Conceptual model

Tokens are the only place visual values live. Every value in every view and every control template is a reference to a token — never a literal. One token change propagates to every consumer. This is not a preference; it is the only correct state of the codebase.

There are two token layers:

**Primitive** — Raw values. These are never referenced directly in views or control files. They exist only so semantic tokens can be defined. In this project, primitives are inlined directly into semantic token definitions (inline hex in Colors.xaml) rather than maintained as a separate named set. Don't introduce a separate primitive layer.

**Semantic** — Meaning-based names. These are what views, styles, and control templates reference. Named by role (`Color.Text.Default`), not by appearance (`Color.Gray700`). A semantic token communicates intent; a viewer of the XAML can understand why that value is being used.

---

## File-by-file rules

### `Colors.xaml`

**Only `SolidColorBrush` resources. Each has an inline hex value. Nothing else.**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Brand -->
    <SolidColorBrush x:Key="Color.Brand.Primary"  Color="#2589E0"/>
    <SolidColorBrush x:Key="Color.Brand.Dark"     Color="#1A6FC4"/>
    <SolidColorBrush x:Key="Color.Brand.Light"    Color="#EBF4FC"/>
    <SolidColorBrush x:Key="Color.Brand.Border"   Color="#B3D4F5"/>

    <!-- Text -->
    <SolidColorBrush x:Key="Color.Text.Default"   Color="#1A2332"/>
    <SolidColorBrush x:Key="Color.Text.Muted"     Color="#6B7A8D"/>
    <SolidColorBrush x:Key="Color.Text.Disabled"  Color="#A0ADB8"/>
    <SolidColorBrush x:Key="Color.Text.OnBrand"   Color="#FFFFFF"/>
    <SolidColorBrush x:Key="Color.Text.Danger"    Color="#C0392B"/>

    <!-- Surface -->
    <SolidColorBrush x:Key="Color.Surface.Default"  Color="#FFFFFF"/>
    <SolidColorBrush x:Key="Color.Surface.Page"     Color="#F4F6F9"/>
    <SolidColorBrush x:Key="Color.Surface.Subtle"   Color="#F0F4F8"/>
    <SolidColorBrush x:Key="Color.Surface.Overlay"  Color="#000000"/>

    <!-- Border -->
    <SolidColorBrush x:Key="Color.Border.Default"   Color="#D1D9E0"/>
    <SolidColorBrush x:Key="Color.Border.Strong"    Color="#9AACBB"/>
    <SolidColorBrush x:Key="Color.Border.Brand"     Color="#B3D4F5"/>

    <!-- Semantic states -->
    <SolidColorBrush x:Key="Color.Success.Default"  Color="#27AE60"/>
    <SolidColorBrush x:Key="Color.Success.Surface"  Color="#EAFAF1"/>
    <SolidColorBrush x:Key="Color.Warning.Default"  Color="#F39C12"/>
    <SolidColorBrush x:Key="Color.Warning.Surface"  Color="#FEF9E7"/>
    <SolidColorBrush x:Key="Color.Danger.Default"   Color="#E74C3C"/>
    <SolidColorBrush x:Key="Color.Danger.Surface"   Color="#FDEDEC"/>
    <SolidColorBrush x:Key="Color.Info.Default"     Color="#2589E0"/>
    <SolidColorBrush x:Key="Color.Info.Surface"     Color="#EBF4FC"/>

</ResourceDictionary>
```

**Rules:**
- Every entry is a `SolidColorBrush` with an inline hex color. That's it.
- No `Color` resources (type: `System.Windows.Media.Color` struct). Colors.xaml produces brushes, not structs.
- No `<SolidColorBrush Color="{StaticResource ...}"/>` — a brush cannot reference another brush through its `Color` property (type mismatch; see critical type rule below).
- No styles, no templates, no converters — this file has one job.
- If a new semantic role is genuinely needed, add it here with an inline hex. Don't define it in a view or chrome file.

---

### `Typography.xaml`

**`FontFamily` resources, `sys:Double` font sizes, `FontWeight` structs.**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">

    <!-- Families -->
    <FontFamily x:Key="Font.Family.Default">pack://application:,,,/Fonts/#Lato</FontFamily>
    <FontFamily x:Key="Font.Family.Mono">Consolas</FontFamily>

    <!-- Sizes (sys:Double — binds directly to FontSize property) -->
    <sys:Double x:Key="Font.Size.Xs">10</sys:Double>
    <sys:Double x:Key="Font.Size.Sm">12</sys:Double>
    <sys:Double x:Key="Font.Size.Base">14</sys:Double>
    <sys:Double x:Key="Font.Size.Md">16</sys:Double>
    <sys:Double x:Key="Font.Size.Lg">18</sys:Double>
    <sys:Double x:Key="Font.Size.Xl">22</sys:Double>
    <sys:Double x:Key="Font.Size.2Xl">28</sys:Double>

    <!-- Weights (FontWeight struct) -->
    <FontWeight x:Key="Font.Weight.Regular">Normal</FontWeight>
    <FontWeight x:Key="Font.Weight.Medium">Medium</FontWeight>
    <FontWeight x:Key="Font.Weight.SemiBold">SemiBold</FontWeight>
    <FontWeight x:Key="Font.Weight.Bold">Bold</FontWeight>

</ResourceDictionary>
```

**Rules:**
- Font sizes must be `sys:Double`. `FontSize` is a `double` property — it can receive a `StaticResource` of type `sys:Double` directly. Do not use `x:Static` or any other mechanism.
- Font families reference the embedded font. If the font pack URI changes, change it here only.
- No inline `FontSize="14"` anywhere in views or styles — always `FontSize="{StaticResource Font.Size.Base}"`.

---

### `Spacing.xaml`

**`Thickness` resources on the 4px base scale.**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Thickness x:Key="Spacing.1">4</Thickness>
    <Thickness x:Key="Spacing.2">8</Thickness>
    <Thickness x:Key="Spacing.3">12</Thickness>
    <Thickness x:Key="Spacing.4">16</Thickness>
    <Thickness x:Key="Spacing.5">20</Thickness>
    <Thickness x:Key="Spacing.6">24</Thickness>
    <Thickness x:Key="Spacing.8">32</Thickness>
    <Thickness x:Key="Spacing.10">40</Thickness>
    <Thickness x:Key="Spacing.12">48</Thickness>

</ResourceDictionary>
```

**Rules:**
- All four sides of a `Thickness` are set to the same value by a single number (uniform). If you need asymmetric padding (e.g. `16,8`), use a `Thickness` with explicit sides — but first ask whether a layout container (`Grid`, `StackPanel` with uniform spacing) would eliminate the need for asymmetric padding.
- The scale is 4px-based. Do not add `Spacing.1.5` (6px) or other off-scale values. If a design calls for 6px, use `Spacing.1` (4) or `Spacing.2` (8) — the scale wins.
- No `Margin="8"` literals anywhere. Always `Margin="{StaticResource Spacing.2}"`.

---

### `Sizing.xaml`

**`sys:Double` resources for heights, widths, icon sizes, touch targets. `CornerRadius` resources for radii.**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">

    <!-- Touch targets (minimum tappable height) -->
    <sys:Double x:Key="Size.TouchTarget.Min">44</sys:Double>
    <sys:Double x:Key="Size.TouchTarget.Sm">36</sys:Double>

    <!-- Icon sizes -->
    <sys:Double x:Key="Size.Icon.Sm">16</sys:Double>
    <sys:Double x:Key="Size.Icon.Default">20</sys:Double>
    <sys:Double x:Key="Size.Icon.Lg">24</sys:Double>

    <!-- Corner radii (CornerRadius struct) -->
    <CornerRadius x:Key="Radius.Sm">4</CornerRadius>
    <CornerRadius x:Key="Radius.Default">8</CornerRadius>
    <CornerRadius x:Key="Radius.Lg">12</CornerRadius>
    <CornerRadius x:Key="Radius.Xl">16</CornerRadius>
    <CornerRadius x:Key="Radius.Full">9999</CornerRadius>

</ResourceDictionary>
```

**Rules:**
- `Size.TouchTarget.Min` (44px) is the minimum height for every interactive element. Mouse-only is not an acceptable reason to go below 44px — this is a touch-compatible POS, not a desktop-only app.
- `CornerRadius` resources are type `CornerRadius`. They bind to `CornerRadius="{StaticResource Radius.Default}"` on `Border` and `ButtonChrome`. Do not use `CornerRadius="8"` literals.
- Do not add sizing tokens for one-off values. If a value appears in exactly one place, it's a local size, not a token.

---

### `Shadows.xaml`

**`DropShadowEffect` resources with inline hex values only.**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <DropShadowEffect x:Key="Shadow.Sm"
        Color="#0F2A4A" Opacity="0.08" BlurRadius="8" ShadowDepth="2" Direction="270"/>

    <DropShadowEffect x:Key="Shadow.Default"
        Color="#0F2A4A" Opacity="0.14" BlurRadius="16" ShadowDepth="4" Direction="270"/>

    <DropShadowEffect x:Key="Shadow.Lg"
        Color="#0F2A4A" Opacity="0.20" BlurRadius="32" ShadowDepth="8" Direction="270"/>

    <DropShadowEffect x:Key="Shadow.None"
        Color="#000000" Opacity="0" BlurRadius="0" ShadowDepth="0"/>

</ResourceDictionary>
```

**Rules:**
- `DropShadowEffect.Color` expects a `System.Windows.Media.Color` struct. `Color.Brand.Primary` in `Colors.xaml` is a `SolidColorBrush`. Referencing it via `StaticResource` on `Color=` is a type mismatch and will fail at runtime without a clear error. **Always use inline hex for `DropShadowEffect.Color`.**
- Do not add a new shadow level without a real use case. Three levels (Sm/Default/Lg) and a None cover the entire app.

---

## Critical type rule — read this before touching any token

WPF's resource system is type-safe. These two types are not interchangeable:

| Type | C# Name | Used for |
|---|---|---|
| `Color` struct | `System.Windows.Media.Color` | `DropShadowEffect.Color`, `GradientStop.Color` |
| `SolidColorBrush` | `System.Windows.Media.SolidColorBrush` | `Background`, `Foreground`, `BorderBrush`, `Fill`, `Stroke` |

`Colors.xaml` produces `SolidColorBrush` resources. Anything that expects a `Color` struct — like `DropShadowEffect.Color` — cannot take a `SolidColorBrush` from `Colors.xaml`. Use inline hex for those.

```xml
<!-- ALWAYS WRONG — Color property receives a brush, fails silently or crashes -->
<SolidColorBrush x:Key="Alias" Color="{StaticResource Color.Brand.Primary}"/>
<DropShadowEffect Color="{StaticResource Color.Brand.Primary}"/>

<!-- ALWAYS RIGHT -->
<SolidColorBrush x:Key="Color.Brand.Primary" Color="#2589E0"/>          <!-- Colors.xaml -->
<DropShadowEffect Color="#0F2A4A" Opacity="0.14" BlurRadius="16"/>      <!-- Shadows.xaml -->
<Border Background="{StaticResource Color.Brand.Primary}"/>              <!-- view/style -->
```

---

## Adding a new token

### New color

Only add to `Colors.xaml` if the role is genuinely new — not representable by an existing key. Check the full key list first. If the existing `Color.Text.Muted` fits your use case, use it — don't define `Color.Text.Secondary` with the same hex.

Pattern: `Color.<Category>.<Role>` — e.g. `Color.Brand.Pressed`, `Color.Surface.Hover`.

### New spacing value

Don't add off-scale values. If 6px is needed, use 4 or 8. If the design consistently calls for a value not on the scale, the design is wrong, not the scale.

### New shadow

Add to `Shadows.xaml` only if the existing three levels genuinely don't fit. Use inline hex.

### New size

Add to `Sizing.xaml` if a value appears in more than one place (otherwise it's a local value). Use `sys:Double` for pixel sizes, `CornerRadius` for radii.

---

## Key naming convention

```
Color.<Category>.<Role>          Color.Brand.Primary
                                 Color.Text.Muted
                                 Color.Surface.Page
                                 Color.Danger.Default

Font.Family.<Name>               Font.Family.Default
Font.Size.<Scale>                Font.Size.Base
Font.Weight.<Name>               Font.Weight.SemiBold

Spacing.<Scale>                  Spacing.4

Size.<Category>.<Scale>          Size.TouchTarget.Min
                                 Size.Icon.Default

Radius.<Scale>                   Radius.Default
                                 Radius.Xl

Shadow.<Scale>                   Shadow.Default
                                 Shadow.Lg
```

No abbreviations. No Hungarian notation. No per-screen prefixes (no `Cashier.Color.X` — tokens are global).

---

## Don't

- Don't add a `Color` struct resource to `Colors.xaml` — it produces brushes only.
- Don't define any token in a view file, a chrome file, or a control file. Tokens live in `Themes/Tokens/` only.
- Don't add a new token without first checking whether an existing token covers the role.
- Don't use `Color="{StaticResource ...}"` on any `SolidColorBrush` or `DropShadowEffect`. Ever.
- Don't use `FontSize="14"` in any style or view — always `{StaticResource Font.Size.Base}`.
- Don't invent an off-scale spacing value — the 4px scale is the rule.

---

## Related files

`skills/wpf-styling/SKILL.md` — how tokens are consumed by control styles and chrome  
`skills/wpf-view-rebuild/SKILL.md` — migrating a view to use tokens  
`Themes/Tokens/Colors.xaml` — brush source of truth  
`Themes/Tokens/Shadows.xaml` — shadow source of truth  
`App.xaml` — merge order
