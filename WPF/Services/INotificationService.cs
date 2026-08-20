using System.Collections.ObjectModel;

namespace UI.Services
{
    /// <summary>
    /// Non-modal, ViewModel-testable replacement for informational
    /// <c>MessageBox.Show</c> calls (errors, success, warnings, info).
    /// <para>
    /// This is NOT for confirmations (Yes/No, OK/Cancel) — those remain
    /// <c>MessageBox.Show</c> by design, since they need to block for a
    /// user decision. This service is for one-way, auto-dismissing status
    /// notifications only.
    /// </para>
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// The live set of currently-visible toasts. <c>ToastHost</c> binds
        /// to this collection to render the stack.
        /// </summary>
        ReadOnlyObservableCollection<ToastMessage> Toasts { get; }

        void ShowSuccess(string message);

        void ShowError(string message);

        void ShowWarning(string message);

        void ShowInfo(string message);

        /// <summary>
        /// Dismisses a specific toast before its auto-dismiss timer elapses
        /// (e.g. user clicks the close affordance).
        /// </summary>
        void Dismiss(ToastMessage toast);
    }
}
