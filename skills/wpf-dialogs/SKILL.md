---
name: wpf-dialogs
description: Defines how to open, close, and get a result back from a WPF dialog — the `IDialogService` abstraction, which of the two closing mechanisms to use, and when a native `Window`/`Microsoft.Win32` dialog is the right call instead. Use whenever opening a new dialog, adding a new dialog type, or touching how an existing dialog closes. This is prescriptive: it names the correct pattern going forward, not a survey of every pattern currently in the codebase — see the Known Inconsistency section for what exists today and how to treat it.
---

# WPF Dialogs

## The correct pattern — always go through `IDialogService`

Every dialog is opened through `IDialogService`, never by constructing a `Window` by hand at the call site:

```csharp
// Fire-and-forget dialog (form, confirmation with its own commands)
_dialogService.ShowDialog<ProductFormView>(formVm);

// Dialog whose outcome the caller needs (confirm/cancel, manager override)
bool? confirmed = _dialogService.ShowDialogWithResult<CardPaymentConfirmDialog>(confirmViewModel);
if (confirmed == true) { /* proceed */ }
```

This is the whole reason `IDialogService` exists: it owns owner-window assignment (`SetOwner`, which guards against the "cannot set Owner to itself" exception when `Application.Current.MainWindow` is unset, already closed, or the window itself) in one place, so no call site has to reimplement it, and it keeps the "how do I show a dialog" decision uniform across the app instead of every ViewModel inventing its own variant.

**Never construct the dialog `Window` directly** (`new SomeDialogWindow { DataContext = vm }; window.Owner = ...; window.ShowDialog();`) for a new dialog. If you're about to write that, use `_dialogService.ShowDialog<T>()` or `ShowDialogWithResult<T>()` instead — there is no case where hand-rolling this for a custom app dialog is the right call going forward (see the native-dialog exception below for the one real exception).

## Choosing `ShowDialog` vs `ShowDialogWithResult`

- **`ShowDialogWithResult<TView>(vm)` — the caller needs to branch on the outcome.** Use this whenever the code that opened the dialog does something different depending on whether the user confirmed or cancelled (a payment confirmation, a manager-override login). The `bool?` return is the dialog's own `Window.DialogResult` — the dialog sets this itself (see closing mechanisms below), the caller just reads it.
- **`ShowDialog<TView>(vm)` — fire-and-forget.** Use this when the dialog's own ViewModel fully owns what happens on save/cancel (it calls the BLL, updates its own state, and the parent simply needs to refresh afterward — see `wpf-crud-screen`'s add/edit flow). The caller doesn't need a `bool?`; it just calls `RefreshDataAsync()` unconditionally after the dialog returns, since the dialog ViewModel itself decided whether anything was actually saved.

If you're unsure which fits: does the code immediately after `ShowDialog(...)` need an `if` on the result? If yes, use `ShowDialogWithResult`. If the line after is just "refresh" or nothing, use `ShowDialog`.

## Closing a dialog from its own ViewModel

The dialog's ViewModel needs a way to tell its Window to close, without the ViewModel holding a direct reference to a `Window` (that would break the MVVM boundary `wpf-mvvm-viewmodel` establishes — a ViewModel shouldn't know about View types it's hosted in).

**Use a `RequestClose` action property**, the simpler and more directly reusable of the two mechanisms already in use in this codebase:

```csharp
// In the dialog ViewModel
public Action? RequestClose { get; set; }

private void Cancel() => RequestClose?.Invoke();
private async Task SaveAsync()
{
    var result = await _someService.AddAsync(...);
    if (result.IsSuccess)
        RequestClose?.Invoke();
    else
        ErrorMessage = result.Error;
}
```

The dialog's code-behind (or the code that constructs it, if going through `IViewModelFactory` before `IDialogService`) wires `vm.RequestClose = () => window.Close();` once, right after construction. Prefer this over the alternative `event Action? DialogClosed` pattern also present in the codebase (`ProductFormViewModel`) — a plain settable `Action` property is simpler to wire from a single call site and doesn't require `+=`/unsubscription bookkeeping for something that's only ever going to have one subscriber (the one Window hosting this one ViewModel instance). Don't introduce a third closing mechanism; if you're touching `ProductFormViewModel`'s `DialogClosed` event for an unrelated reason, leave it as-is rather than converting it mid-task (`AGENTS.md` Scope Discipline).

For a dialog whose result the caller reads via `ShowDialogWithResult`, set `Window.DialogResult` (`true`/`false`) before/instead of closing — WPF closes the window automatically once `DialogResult` is set on a window shown via `ShowDialog()`. A plain `IsCancel="True"` on a Cancel button in XAML is a legitimate zero-code way to wire simple cancel-and-close behavior without touching the ViewModel at all — use it for a straightforward "Cancel just closes, nothing to clean up" button rather than adding a `CancelCommand` that does the same thing through a longer path.

## The one real exception — native OS dialogs

`Microsoft.Win32.SaveFileDialog` / `OpenFileDialog` / `FolderBrowserDialog` are **not** app dialogs and don't go through `IDialogService` — they have no `DataContext`/ViewModel to host, and `IDialogService`'s `TView : Window, new()` constraint doesn't fit them anyway. Call `.ShowDialog()` on them directly, as `ReportViewModel.Commands.cs`'s Excel export does. This is the only sanctioned case for calling `ShowDialog()` outside `IDialogService` — don't use it as precedent for a custom app dialog.

## Known inconsistency in the existing codebase — don't extend it

Several existing dialogs (`TranslationDialogView`, `AddEditCategoryDialog`, and others opened from `SizeManagementViewModel`, `ProductFormViewModel`'s nested translations dialog, `CategoryManagementViewModel`, `ModifierGroupManagementViewModel`, `ShiftManagementViewModel.Actions`, `ReceiptManagementViewModel`) bypass `IDialogService` and hand-roll `new DialogWindow { DataContext = vm }` plus a manually duplicated copy of the owner-assignment guard, using the `RequestClose` mechanism to close. This is legacy, not a second sanctioned pattern:

- **Don't copy this shape for a new dialog.** Use `IDialogService` as described above.
- **Don't silently rewrite these existing call sites** to use `IDialogService` as a drive-by fix while working on something unrelated — that's a real refactor with its own blast radius (`AGENTS.md` Scope Discipline; run `cross-surface-impact-check` if you do take it on deliberately as its own task). Flag it, don't fix it in passing.
- If you're asked to add a new dialog to one of these same ViewModels (e.g. another translation-style dialog on a screen that already has one opened the old way), prefer `IDialogService` for the *new* one even if the sibling dialog on the same screen still uses the old pattern — consistency with the rest of the app matters more than local consistency with one legacy call site.

## Don't

- Don't construct a custom app dialog `Window` directly at the call site — always `IDialogService`.
- Don't give a dialog ViewModel a direct `Window` reference or `System.Windows` dependency to close itself — use `RequestClose`.
- Don't add a third closing mechanism alongside `RequestClose` and `DialogClosed` — use `RequestClose` for anything new.
- Don't route native `SaveFileDialog`/`OpenFileDialog` through `IDialogService` — call them directly, that's the sanctioned exception.

## Related files

`skills/wpf-mvvm-viewmodel/SKILL.md` (why a ViewModel shouldn't hold a `Window` reference), `skills/wpf-crud-screen/SKILL.md` (the add/edit dialog flow this skill's `ShowDialog` guidance directly supports), `skills/wpf-navigation/SKILL.md` (dialogs vs. page navigation — this skill is the dialog half), `skills/cross-surface-impact-check/SKILL.md` (if deliberately migrating a legacy raw-`Window` dialog).
