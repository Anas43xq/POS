---
name: wpf-crud-screen
description: Defines the end-to-end pattern for a management-style CRUD screen in WPF — Products, Categories, Sizes, Modifier Groups and anything shaped like them. Covers the management-list ViewModel's Crud partial, the form-dialog ViewModel, `CrudActionButtons`, delete confirmation, and post-mutation refresh. Use whenever adding create/update/delete to a WPF screen, or adding a brand-new management page. For non-CRUD screens (reports, dashboards, read-only views) see `wpf-view-creation` instead. For generic ViewModel mechanics (BaseViewModel, RunAsync, Dispose) see `wpf-mvvm-viewmodel`.
---

# WPF CRUD Screen

## When to use this

Any WPF screen where a user adds, edits, or deletes records from a list — matches the existing `ProductManagementViewModel`, `CategoryManagementViewModel`, `SizeManagementViewModel`, `ModifierGroupManagementViewModel`. Also read this before adding create/update/delete to a screen that's currently read-only. This is the WPF-side equivalent of "the operation is a real business mutation," matching the CRUD BLL methods `bll-dal-service-creation` already assumes exist (`AddXAsync`/`UpdateXAsync`/`DeleteXAsync`).

## The shape, end to end

```text
Management ViewModel (list + selection)
  └─ AddCommand/EditCommand  → opens a Form ViewModel via IViewModelFactory → IDialogService.ShowDialog<FormView>
  └─ DeleteCommand           → MessageBox confirm → BLL DeleteXAsync → RefreshDataAsync
Form ViewModel (single-record add/edit)
  └─ SaveCommand → BLL AddXAsync/UpdateXAsync → closes dialog → parent RefreshDataAsync
  └─ CancelCommand → closes dialog, discards
```

### Management ViewModel — list side

- Keep the CRUD-specific methods in a separate partial class file, `<Name>ManagementViewModel.Crud.cs`, alongside the main `<Name>ManagementViewModel.cs` — matches `ProductManagementViewModel.Crud.cs`. This is the same partial-class split called out in `wpf-mvvm-viewmodel`'s SOLID section, applied specifically to CRUD.
- `Add<X>()` — synchronous, opens the form dialog for a new record: `_viewModelFactory.Create<XFormViewModel>(this)`, then `_dialogService.ShowDialog<XFormView>(formVm)`. Pass `this` (the parent) so the form can call back into the parent's refresh once saved.
- `Edit<X>()` — same, but also passes the selected row: `_viewModelFactory.Create<XFormViewModel>(this, SelectedX)`. Guard on `SelectedX == null` and return early — don't rely solely on the command's `CanExecute` to prevent this, since `CanExecute` only disables the UI trigger, not a direct call path.
- `Delete<X>Async()` — always confirm first with `MessageBox.Show(..., MessageBoxButton.YesNo, MessageBoxImage.Warning)` before calling the BLL delete method. Check `result == MessageBoxResult.Yes` (or explicitly `!= Yes` to early-return) — never delete on any button other than an explicit Yes. After a successful delete, call `RefreshDataAsync()` — don't just remove the item from the in-memory collection, since that risks the UI drifting from what's actually in the database if the delete BLL call didn't do what was expected.
- `CanEdit<X>()`/`CanDelete<X>()` — both are simply `SelectedX != null`, wired as the `CanExecute` predicate on the respective `RelayCommand`/`AsyncRelayCommand`.
- `RefreshDataAsync()` — resets any active filters/search text and selection, then reloads (matches `ProductManagementViewModel.Crud.cs`'s `RefreshDataAsync`, which clears `CategorySearchText`/`ProductSearchText`/`SelectedCategory`/`SelectedProduct` before calling `LoadDataAsync()`). Call this after every successful add/edit/delete, not just delete — a stale list after a successful add is the same class of bug as one after a delete.

### Form ViewModel — single-record side

Two divergent patterns currently exist in the codebase for this — **prefer the `ProductFormViewModel` pattern for new screens**, and treat `AddEditCategoryViewModel`'s pattern as legacy, not something to copy:

- **Preferred (`ProductFormViewModel`)**: constructed exclusively through `IViewModelFactory.Create<T>(parentVm, existingRecordOrNothing)`, with all services (`IProductService`, `ICategoryService`, etc.) required (non-nullable) constructor parameters resolved from DI. No parameterless constructor. This matches the DI-first approach the rest of the app uses and is what `wpf-mvvm-viewmodel`'s factory guidance assumes.
- **Legacy, don't replicate (`AddEditCategoryViewModel`)**: nullable service parameters with a parameterless constructor fallback (`: this(null, null, null)`), presumably for XAML design-time data binding. If you're touching this specific ViewModel for an unrelated reason, leave the pattern as-is per `AGENTS.md` Rule 2 ("don't rewrite what already works") — don't silently migrate it mid-task. If you're asked to build a *new* form ViewModel, use the `ProductFormViewModel` pattern, not this one.
- `SaveCommand` (`RelayCommand`/`AsyncRelayCommand`, `CanSave` as the `CanExecute` predicate covering required-field validation) calls the appropriate BLL `AddXAsync`/`UpdateXAsync`, and on `Result<T>.IsSuccess` closes the dialog and signals the parent to refresh. On failure, surface `Result<T>.Error` — via `RunAsync<T>` (see `wpf-mvvm-viewmodel`) if the shape fits, or the ViewModel's own `ErrorMessage`/`HasError` bound properties (as `AddEditCategoryViewModel` does) if a dedicated inline validation display is more appropriate for that form.
- `CancelCommand` closes the dialog without saving — a plain `RelayCommand`, no confirmation needed (unlike delete) since cancelling a form isn't destructive to existing data.
- Determine Add-vs-Edit mode from whether an existing record was passed into the constructor (`_existingProduct is null` in `ProductFormViewModel`, `CategoryId > 0` in `AddEditCategoryViewModel`) — set the dialog title (`DialogTitle`) and whether fields are pre-populated based on this, rather than having the caller pass a separate `isEditMode` flag redundant with "was a record passed."

## `CrudActionButtons` — always reuse it for the toolbar

The Add/Edit/Delete/Refresh (and optional Translations) button row is a shared control (`WPF/Controls/CrudActionButtons.xaml`) — see `wpf-reusable-controls` for its DependencyProperty conventions. For any CRUD screen's toolbar, bind to this control rather than hand-building buttons:

```xml
<controls:CrudActionButtons
    AddCommand="{Binding AddCommand}" EditCommand="{Binding EditCommand}"
    DeleteCommand="{Binding DeleteCommand}" RefreshCommand="{Binding RefreshCommand}"
    EditEnabled="{Binding CanEditProduct}" DeleteEnabled="{Binding CanDeleteProduct}" />
```

Set `ShowRefresh="False"` for an inline/embedded use that doesn't own its own refresh (e.g. a section header inside a larger page where the page-level toolbar already has Refresh). Set `ShowTranslations="True"` only if the entity has a translation dialog (see `SizeManagementViewModel`'s `TranslationsCommand`) — most CRUD screens don't need this.

## Wiring a brand-new CRUD screen

1. BLL: confirm `AddXAsync`/`UpdateXAsync`/`DeleteXAsync` exist on the relevant service — if not, that's `bll-dal-service-creation` territory, done first.
2. Create `<Name>ManagementViewModel.cs` (list, load, filter/search) + `<Name>ManagementViewModel.Crud.cs` (partial, the four CRUD methods above) + `<Name>FormViewModel.cs` (single-record add/edit, `ProductFormViewModel` pattern).
3. Create the matching Views: `<Name>ManagementView.xaml` (list + `CrudActionButtons` toolbar) and `<Name>FormView.xaml` (dialog).
4. Register both ViewModels `AddTransient` in `App.xaml.cs` (see `wpf-mvvm-viewmodel`'s DI section).
5. Wire navigation and the `DataTemplate` mapping — see `wpf-navigation`.
6. Follow `wpf-styling` for the form/dialog chrome (`DialogChrome.xaml`) and button styles (`Buttons.xaml`) rather than hand-rolling new ones.

## Don't

- Don't delete without a confirmation dialog — every existing delete path confirms first; a silent delete is a regression from established UX, not a simplification.
- Don't mutate the in-memory list directly after Add/Edit/Delete instead of calling `RefreshDataAsync()` — the UI must reflect what the database actually has, not an optimistic local edit.
- Don't build a new form ViewModel on the `AddEditCategoryViewModel` nullable-services/parameterless-constructor pattern — that's legacy, not the template to copy (see above).
- Don't hand-build Add/Edit/Delete/Refresh buttons per-screen instead of `CrudActionButtons` — that's exactly the duplication `reusable-components`-style thinking exists to prevent (see `wpf-reusable-controls`).

## Related files

`skills/wpf-mvvm-viewmodel/SKILL.md` (ViewModel/command internals), `skills/wpf-view-creation/SKILL.md` (non-CRUD screens), `skills/wpf-navigation/SKILL.md` (registering the new page), `skills/wpf-reusable-controls/SKILL.md` (`CrudActionButtons` conventions), `skills/wpf-styling/SKILL.md` (dialog/button chrome), `skills/bll-dal-service-creation/SKILL.md` (adding the BLL mutation methods if missing), `AGENTS.md` Rule 2.
