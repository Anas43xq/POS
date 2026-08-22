# AGENTS.md — WPF POS

This file is the working contract for any agent operating inside `WPF/`. Read this before touching anything. Every rule here is non-negotiable.

---

## Context

The WPF POS UI went through a full visual-layer rebuild — token system, control library, chrome, and all view files. **The rebuild is complete.** This file now governs ongoing WPF work: new screens, edits to existing screens, and any styling/ViewModel change. ViewModels, business logic (BLL/DAL), navigation, data bindings, and converters remain a separate concern from the visual layer (see "What styling work must not touch" below) — a screen change may need both, but the token/styling rules below apply to the visual layer regardless of which kind of change is in flight.

The token system exists because the original UI had no system: hardcoded values in 3–4 places simultaneously, WPF dependency property precedence picking silent winners, bugs that couldn't be traced. Every visual value has one source of truth. This is now the permanent standard, not a rebuild-only target — any new screen or edit follows it from the first line.

## Durable memory

`Memory.md` is the durable session record. Update it whenever you make a lasting decision, confirm a rule, discover an open issue, or finish work that should survive beyond the current turn. Record the actual outcome, not the exploration path. If a new rule changes how future WPF work should be done, it belongs in `Memory.md` before handoff.

`KownIssues.md` holds the compact history of WPF runtime/build errors and their fixes. Check it before debugging XAML parse errors, missing resources, object namespace text in rows, or old resource alias regressions.

When writing to `KownIssues.md`, keep it compact and reusable: record the symptom, root cause, and final fix/rule in one short bullet under the closest existing section. Do not paste logs, exploration notes, stack traces, or long timelines. If the issue creates a rule future agents must follow, add the rule to `Memory.md` too and leave the detailed failure note in `KownIssues.md`.

---

## The one rule that overrides everything else

**If a value is hardcoded anywhere in a view file, the file is not done.**

Zero inline hex. Zero `Foreground="White"`. Zero `FontSize="14"`. Zero `Margin="8,0,0,0"`. Every visual value is a token reference. This is not a target — it is the definition of "done." Applies to every new view and every edit to an existing view, not just rebuild work.

---

## Skill routing — use the right skill for the task

| Task | Skill |
|---|---|
| Writing or editing any token file (`Colors.xaml`, `Typography.xaml`, `Spacing.xaml`, `Sizing.xaml`, `Shadows.xaml`) | `skills/wpf-token-system/SKILL.md` |
| Writing or editing any control style, chrome style, or rule about what's allowed in views | `skills/wpf-styling/SKILL.md` |
| Reworking an existing view file's visual layer | `skills/wpf-view-rebuild/SKILL.md` |
| Creating a new non-CRUD screen such as a dashboard, report, or read-only summary view | `skills/wpf-view-creation/SKILL.md` |
| Any ViewModel — base class, commands, DI, lazy loading, `Dispose` | `skills/wpf-mvvm-viewmodel/SKILL.md` |
| A screen with add/edit/delete | `skills/wpf-crud-screen/SKILL.md` |
| Making a screen reachable — page switching, dialog launch | `skills/wpf-navigation/SKILL.md` |
| Opening, closing, or getting a result from a dialog | `skills/wpf-dialogs/SKILL.md` |
| A shared `UserControl`, `IValueConverter`, or attached behavior | `skills/wpf-reusable-controls/SKILL.md` |
| Performance — slow screen, large list, threading | `skills/wpf-performance/SKILL.md` |

Most visual-layer tasks touch `wpf-styling` + `wpf-view-rebuild`. Token file work touches `wpf-token-system` + `wpf-styling`. Read all that apply.

---

## Rebuild history — complete

All phases below are done. Kept for reference so the reasoning behind the current token/control/chrome structure isn't lost; do not re-open a phase to "redo" it without an explicit task asking for that.

| Phase | Scope | Status |
|---|---|---|
| **0** | Token files: `Colors.xaml`, `Typography.xaml`, `Spacing.xaml`, `Sizing.xaml`, `Shadows.xaml` + `App.xaml` merge order | ✓ Done |
| **1** | Control library: `Buttons.xaml`, `Inputs.xaml`, `DataGrid.xaml`, `Lists.xaml`, `CheckRadio.xaml`, `Dialogs.xaml` | ✓ Done |
| **2** | Chrome: `AppChrome.xaml`, `CashierChrome.xaml` | ✓ Done |
| **3** | Cashier surface: all cashier views | ✓ Done |
| **4** | Manager surface: all manager views | ✓ Done |
| **5** | Shared / auth / dialogs: `LoginWindow`, `ManagerPinOverlayView`, `TranslationDialogView` | ✓ Done |
| **6** | Verification | ✓ Done — see "Verification checklist" below; last full pass logged in `Memory.md` |

For new screens or major reworks going forward, follow the same discipline (tokens first, no local `<Style>`, verification checklist clean) rather than treating this as a closed chapter with looser rules.

---

## Hard rules — agent must refuse tasks that violate these

### Never allowed in any view or control file

```
Foreground="..."                         → style setter or token reference only
Background="..."                         → style setter or token reference only
BorderBrush="..."                        → style setter or token reference only
FontSize="..."                           → {StaticResource Font.Size.X}
FontFamily="..."                         → {StaticResource Font.Family.Default}
Margin="8,0,0,0"                        → {StaticResource Spacing.2} or token
Padding="12,8"                          → token
CornerRadius="8"                        → {StaticResource Radius.Default}
Color="#..."                            → only allowed in Colors.xaml and Shadows.xaml
<SolidColorBrush Color="{StaticResource → type mismatch — always wrong
DropShadowEffect Color="{StaticResource → type mismatch — always wrong
<Style> in App.xaml                     → not allowed
<Style> in a view file                  → not allowed
```

### Always required

```
Every Button          → Style="{StaticResource POS.Button.X}"
Every TextBox         → Style="{StaticResource POS.TextBox.Default}"
Every ComboBox        → Style="{StaticResource POS.ComboBox.Default}"
Every DataGrid        → Style="{StaticResource POS.DataGrid.Default}"
Every interactive el. → MinHeight="{StaticResource Size.TouchTarget.Min}"
Foreground on button  → in the style setter only, never on the element
```

### The type rule — critical

`SolidColorBrush` and `Color` (struct) are different WPF types. `Colors.xaml` produces brushes. `DropShadowEffect.Color` and `GradientStop.Color` expect a struct. **Never cross them.** Use inline hex in `Shadows.xaml`. Use brush references (`{StaticResource Color.X.Y}`) for `Background`, `Foreground`, `BorderBrush`.

### The Foreground ownership rule

A local value on an element in a view blocks all style triggers and control template triggers. If `Foreground` is set on a `Button` element in a view, the Disabled trigger in the `POS.Button.Primary` template silently loses. The button stays the wrong color in disabled state. This was the exact class of bug that caused the rebuild. **Foreground is owned by the style setter. Never set it on the element.**

### The icon glyph rule

If a decorative glyph is used in XAML, it must be a valid Unicode character or a proper image asset. Never leave mojibake text such as `âš ` or `ðŸ§¾` in a view. If the visual is still a placeholder for a future image icon, keep the placeholder in place and mark it for replacement later; do not strip the icon location out of the layout.

### Shortcut label formatting rule

When a button displays its keyboard shortcut inline, format it as `Action - Key` with a space-dash-space separator (e.g. `Cash - F3`, `Card - F4`). Use the actual configured shortcut value from the shortcut system (`ShortcutSettings`); do not duplicate or hardcode shortcut key values. The `KeyHint` control satisfies this convention when placed alongside or overlaid on the action label. Do not crowd the action label and shortcut together without a clear separator.

### Localization direction rule

Localization may change text direction for a given language (e.g. Arabic → RTL rendering), but must **not** automatically mirror or rearrange WPF component structure via `FlowDirection` on the window root or any container unless a specific screen is explicitly designed for a mirrored layout. The WPF Unicode bidirectional algorithm handles Arabic text direction in `TextBlock` elements automatically. Do not call `MainWindow.FlowDirection = RightToLeft` from the localization service or any language-change handler. If a future screen genuinely requires a mirrored layout, document that decision explicitly here.

### Receipt exception

`ReceiptWindow.xaml` and `ReceiptPrintView.xaml` are print surfaces, not interactive cashier UI. Fixed numeric layout values are allowed there when the receipt format needs exact spacing or sizing. Do not treat those hardcoded numbers as violations in other views.

### Currency rendering rule

Money amounts in totals, payment panels, receipts, KPI summaries, and standalone money displays must use `controls:CurrencyText`. Transaction/list/table rows must not show the currency glyph or currency symbol; list-row amounts use plain numeric formatting such as `StringFormat={}{0:N2}` so dense lists stay readable. Do not use culture-based currency formatting such as `StringFormat={}{0:C}`, `ToString("C")`, inline currency symbols, or custom one-off money layouts in a view. The `CurrencyText` control owns the AED dirham glyph, amount text, amount color, and icon size wherever currency presentation is required.

### List row display rule

List, check-list, and DataGrid rows must bind to explicit display fields or templates. Never let a row content presenter/details presenter fall back to the bound object itself, because WPF will display namespace/type text such as `Contracts.Transactions.TransactionListItemDto`.

---

## Folder structure

```
WPF/
├── Themes/
│   ├── Tokens/
│   │   ├── Colors.xaml          ← SolidColorBrush only. Inline hex. Single source of truth.
│   │   ├── Typography.xaml      ← FontFamily, sys:Double sizes, FontWeight structs
│   │   ├── Spacing.xaml         ← Thickness keys. 4px base scale.
│   │   ├── Sizing.xaml          ← sys:Double heights/widths. CornerRadius keys.
│   │   └── Shadows.xaml         ← DropShadowEffect with inline hex.
│   ├── Controls/
│   │   ├── Buttons.xaml         ← All Button variants + ControlTemplates. All states.
│   │   ├── Inputs.xaml          ← TextBox, ComboBox, PasswordBox, DatePicker
│   │   ├── DataGrid.xaml        ← DataGrid, row, header, cell
│   │   ├── Lists.xaml           ← ListBox, ListBoxItem, badges, chips
│   │   ├── CheckRadio.xaml      ← CheckBox, RadioButton, ToggleButton
│   │   └── Dialogs.xaml         ← Dialog shell chrome
│   └── Chrome/
│       ├── AppChrome.xaml       ← Manager nav, sidebar, layout
│       └── CashierChrome.xaml   ← Cashier layout, header
├── Views/                        ← Rebuilt screens (same folder structure as before)
├── Controls/                     ← Custom UserControls
├── App.xaml                      ← MergedDictionaries only. Fixed order.
└── ...
```

---

## App.xaml merge order — fixed, never changes

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- ① Tokens — must be first, no dependencies -->
    <ResourceDictionary Source="Themes/Tokens/Colors.xaml"/>
    <ResourceDictionary Source="Themes/Tokens/Typography.xaml"/>
    <ResourceDictionary Source="Themes/Tokens/Spacing.xaml"/>
    <ResourceDictionary Source="Themes/Tokens/Sizing.xaml"/>
    <ResourceDictionary Source="Themes/Tokens/Shadows.xaml"/>

    <!-- ② Utilities — depend on tokens only -->
    <ResourceDictionary Source="Resources/Converters.xaml"/>

    <!-- ③ Controls — depend on tokens only -->
    <ResourceDictionary Source="Themes/Controls/Buttons.xaml"/>
    <ResourceDictionary Source="Themes/Controls/Inputs.xaml"/>
    <ResourceDictionary Source="Themes/Controls/DataGrid.xaml"/>
    <ResourceDictionary Source="Themes/Controls/Lists.xaml"/>
    <ResourceDictionary Source="Themes/Controls/CheckRadio.xaml"/>
    <ResourceDictionary Source="Themes/Controls/Dialogs.xaml"/>

    <!-- ④ Chrome — depends on tokens + controls -->
    <ResourceDictionary Source="Themes/Chrome/AppChrome.xaml"/>
    <ResourceDictionary Source="Themes/Chrome/CashierChrome.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

---

## What styling work must not touch

| Area | Status |
|---|---|
| ViewModels (`WPF/ViewModels/`) | Separate concern — only touch when the task is a ViewModel change |
| Business logic (BLL/DAL) | Separate concern |
| Navigation / routing | Separate concern |
| Data bindings in views | Preserve exactly when doing visual-only work |
| Converters (`Resources/Converters.xaml`) | Separate concern |
| Custom UserControls (`WPF/Controls/`) | Only touch when the task is about that control |
| Localization strings | Separate concern |
| API / backend | Separate concern |

If it binds, it stays as-is during a visual-only pass. If it's a visual value, it becomes a token reference. A task can legitimately span more than one of these areas (e.g. a new feature needing both a ViewModel change and a new view) — this table says what a *styling-only* pass must leave alone, not what a whole feature task is limited to.

---

## Verification checklist — run for any new or changed view

```
grep '="#'                      in Views/  → 0 results
grep 'Foreground="'             in Views/  → 0 results
grep 'Background="'             in Views/  → 0 results
grep 'FontSize="'               in Views/  → 0 results
grep 'Margin="[0-9]'            in Views/  → 0 results
grep 'Color="{StaticResource'   in project → 0 results  (type mismatch)
grep 'StringFormat={}{0:C}\|ToString("C")' in Views/ → 0 results for culture-based currency formatting
grep '<Style '                  in Views/  → 0 results (no local <Style> blocks)
```

After any view change:
- [ ] App starts without `XamlParseException`
- [ ] No missing resource key warnings in output window
- [ ] All button states (default, hover, pressed, disabled, focused) correct
- [ ] 44px minimum touch target on all interactive elements
- [ ] Focus state visible on all keyboard-navigable elements
- [ ] Text on brand backgrounds is `Color.Text.OnBrand` (white)

A known outstanding exception from the rebuild is logged in `Memory.md` — check there before assuming a fresh `<Style>` hit is new.

---

## Old system — gone


Old key names that no longer exist:
`BrandDark`, `BrandMid`, `BrandLight`, `PrimaryGradientBtn`, `OutlineBtn`, `DangerBtn`, `ModalCardBorder`, `DashboardCardBorder`, `DashboardCardTitle`, `FieldLabel`, `CrudNeutralBg`

Current keys follow the pattern: `Color.X.Y`, `Font.X.Y`, `Spacing.N`, `Radius.X`, `Shadow.X`, `POS.Button.X`, `POS.TextBox.Default`, `POS.DataGrid.Default`.

---

## Related files

`skills/wpf-token-system/SKILL.md`
`skills/wpf-styling/SKILL.md`
`skills/wpf-view-rebuild/SKILL.md`
`skills/wpf-view-creation/SKILL.md`
`skills/wpf-mvvm-viewmodel/SKILL.md`
`skills/wpf-crud-screen/SKILL.md`
`skills/wpf-navigation/SKILL.md`
`skills/wpf-dialogs/SKILL.md`
`skills/wpf-reusable-controls/SKILL.md`
`skills/wpf-performance/SKILL.md`
`Memory.md`
`KownIssues.md`

---

## DAL / EF architecture note

When a task intentionally touches DAL or EF architecture as the source of truth. Do not introduce or preserve mixed lifetime patterns unless a task explicitly requires it and the reason is documented. The preferred direction is one consistent factory-based `PosDbContext` usage model, with raw ADO reserved for proven hot paths.

---

## Graphify knowledge graph

The project has a built knowledge graph at `graphify-out/` (code → AST → communities, fully local, no API cost). **Before answering architecture, file-relationship, call-graph, or "how does X connect to Y" questions, prefer a scoped graph query over reading raw files** — it is faster and spans the whole codebase.

```
graphify query  "<question>" --graph graphify-out/graph.json   # BFS traversal, broad context
graphify query  "<question>" --graph graphify-out/graph.json --dfs --budget 4000
graphify explain "<symbol>"                                    # node + its direct connections
graphify path   "A" "B" --graph graphify-out/graph.json --undirected   # path between two nodes
graphify path   "A" "B" --graph graphify-out/graph.json         # directed path if one exists
```

- Ambiguous symbol names (this repo splits ViewModels across `.Partial.cs` / `.Sales.cs`) — retry `explain` with the full node id it prints.
- Code changes: `graphify update .` re-extracts changed code files (AST only, no LLM). Doc/image/report changes need `graphify extract ./docs --update` (needs an API key/backend).
- The `.graphifyignore` in the repo controls what is indexed; `graphify-out/` and `.cline/` are git-ignored (keep the graph local, rebuild on checkout).
- Only grep raw files if a graph query genuinely cannot answer the question.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, use the installed graphify skill or instructions before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
