---
name: wpf-reusable-controls
description: Defines when and how to extract a reusable WPF UserControl, IValueConverter, or attached-property behavior - the three distinct extraction mechanisms under `WPF/Controls/`, `WPF/Converters/`, `WPF/Behaviors/` - and the DependencyProperty conventions to follow. Use whenever the same small piece of UI, value transformation, or cross-cutting XAML behavior would otherwise be duplicated across two or more Views. This is the WPF-side equivalent of the mobile `reusable-components` skill.
---

# WPF Reusable Controls, Converters & Behaviors

## When to use this

Any time a piece of UI markup, a value-to-value binding transformation, or a cross-cutting XAML-attachable behavior is about to be duplicated across a second View. Read this before copy-pasting XAML from one View into another, or before writing a second near-identical `IValueConverter`.

## Three distinct mechanisms - pick the right one

| You need... | Use | Example |
|---|---|---|
| Reusable **UI markup** (visual + optional behavior) | `WPF/Controls/*.xaml` + `.xaml.cs` - a `UserControl` | `CrudActionButtons`, `CurrencyText`, `CurrencyIcon`, `ToastHost`, `DateRangeFilterControl`, `RecentSalesList` |
| Reusable **binding value transformation**, no UI | `WPF/Converters/*.cs` - an `IValueConverter` | `BoolToVisibilityConverter`, `EnumToBooleanConverter`, `ZeroToVisibilityConverter` |
| Reusable **cross-cutting XAML behavior** attachable to any element | `WPF/Behaviors/*.cs` - a static class with attached `DependencyProperty`s | `ShortcutBindingsBehavior`, `FocusExtension` |

Don't reach for a `UserControl` when a `Converter` would do (e.g. don't wrap a single `TextBlock` with a converter-driven `Visibility` binding in its own `UserControl` - that's over-engineering per `code-style`'s "Don't Over-Engineer" section). Don't reach for a `Behavior` when the same result is achievable with a plain data-bound `Command` - behaviors are for cases with no MVVM-friendly binding path (keyboard shortcuts, focus management).

## UserControls (`WPF/Controls/`)

- **Stay ViewModel-agnostic where possible.** `CrudActionButtons` is the model to follow: it owns **no `DataContext`**, exposing every command/flag/label as a `DependencyProperty` that the consuming View binds to its own page ViewModel. This means adopting the control never requires changing any ViewModel, command, or existing binding - see its own header comment for the rationale. Prefer this shape for any new shared control over one that assumes a specific ViewModel type.
- **Exception - self-contained controls that resolve their own dependency.** `ToastHost` is the deliberate exception: it resolves `INotificationService` from `App.ServiceProvider` in its own constructor (guarded by `DesignerProperties.GetIsInDesignMode` so design-time XAML preview doesn't crash without a running container) rather than taking bindable properties, specifically because it's meant to be dropped into any Window with zero wiring. Use this pattern only when the control's whole purpose is "attach me anywhere with no per-page setup" - for anything that varies per-consumer (labels, commands, enabled states), use the `CrudActionButtons` DependencyProperty pattern instead.
- **`DependencyProperty` naming/structure**: `public static readonly DependencyProperty <Name>Property = DependencyProperty.Register(nameof(<Name>), typeof(T), typeof(<ControlName>), new PropertyMetadata(<default>));` followed by the CLR wrapper property (`get => (T)GetValue(...); set => SetValue(...);`). Group related properties with a `// -- <Group> --` comment banner when the control has several (see `CrudActionButtons`' Add/Edit/Delete/Refresh/Translations grouping) - keeps a large control's property list scannable.
- **Boolean toggles for optional sub-parts**: follow `CrudActionButtons`' `ShowRefresh`/`ShowTranslations` pattern for a control that has an optional section some consumers need and others don't - default the flag to whichever state most consumers want (`ShowRefresh` defaults `true` since most toolbars want it; `ShowTranslations` defaults `false` since most entities don't have translations).

### `CurrencyText`

`CurrencyText` is the canonical money renderer for this UI. Use it whenever a view needs to show an amount, total, price, or change value.

- Bind `Amount` to the numeric value. The control formats the number itself, so views should not use culture-based currency formatting such as `StringFormat={}{0:C}` or `ToString("C")`.
- Use `AmountForeground` to tint both the AED dirham glyph and the amount text. This is the property to change for dark brand surfaces or muted totals.
- Use `IconSize` when the glyph needs to be larger or smaller than the amount text. Leave it unset when the default auto-sizing is fine.
- Use the normal `FontSize` and `FontWeight` properties on the control to shape the amount text. The inner `TextBlock` inherits those bindings automatically.
- If you need a different money presentation, adjust the reusable control once. Do not build a per-view money layout in XAML.

## Converters (`WPF/Converters/`)

- One converter, one transformation - matches `code-style`'s "one function, one action" applied to `Convert`/`ConvertBack`. Don't build a single converter that branches on a `parameter` to perform several unrelated transformations; that's effectively several converters hiding in one class.
- Implement `ConvertBack` meaningfully if the binding could plausibly be `TwoWay` (see `BoolToVisibilityConverter`'s `ConvertBack` mapping `Visibility` back to `bool`); if the binding is genuinely one-way only, `ConvertBack` can throw `NotSupportedException` or return `Binding.DoNothing` - don't silently return a wrong/default value that could mask a real binding bug.
- Name converters for the transformation, not the screen that first needed them (`ZeroToVisibilityConverter`, not `ProductCountVisibilityConverter`) - a well-named generic converter is more likely to be reused than a screen-specific one, and this project's existing converters already follow that naming.
- Register in `Resources/Converters.xaml` (merged first in `App.xaml`, see `wpf-styling`) so it's available application-wide as a `StaticResource`, rather than declaring it locally in one View's `Resources`.

## Behaviors (`WPF/Behaviors/`)

- Attached `DependencyProperty` on a `static class`, following `ShortcutBindingsBehavior`'s shape - this is the right tool specifically when there's no clean MVVM binding path for the concern (global keyboard shortcuts sourced from `shortcuts.json`, focus management via `FocusExtension`).
- Document the XAML usage syntax in the class's doc-comment (see `ShortcutBindingsBehavior`'s `<code>` example) since attached-property usage isn't discoverable from IntelliSense the way a normal property is.
- Don't use a behavior as a workaround for "I don't want to add a command to this ViewModel" - if a clean command binding is possible, use it; behaviors are for the genuine no-binding-path cases only.

## Deciding to extract - the trigger

Same discipline as `reusable-components` (mobile): when the same markup/logic is about to exist in a second place, extract before duplicating a third time. If you find a third near-duplicate already existing under a different name while doing this, migrate it too in the same pass rather than leaving it to drift further (matches `reusable-components`' explicit instruction on this).

## Don't

- Don't give a new shared `UserControl` its own `DataContext`/ViewModel unless it's genuinely self-contained like `ToastHost` - default to the `CrudActionButtons` DependencyProperty-only shape.
- Don't duplicate an existing converter's logic under a new name because it was faster than searching - check `WPF/Converters/` first.
- Don't reach for an attached behavior when a plain command binding solves the problem - see above.
- Don't leave a newly-extracted control's old duplicated call sites un-migrated - replace every one in the same pass.

## Related files

`skills/code-style/SKILL.md` (one-function-one-action, don't-over-engineer - applies to converters/behaviors too), `skills/wpf-crud-screen/SKILL.md` (`CrudActionButtons`' primary consumer and canonical usage example), `skills/wpf-styling/SKILL.md` (the resource dictionaries these controls draw brushes/styles from), `skills/reusable-components/SKILL.md` (the mobile-side equivalent - same extraction discipline, different mechanism).
