namespace UI.Services
{
    /// <summary>
    /// Outcome of a manager PIN approval request.
    /// </summary>
    public readonly record struct ManagerApprovalResult(bool Approved, string? Reason);

    /// <summary>
    /// Shows the manager PIN overlay and returns true if the manager
    /// approved the action, false if they cancelled or failed.
    /// </summary>
    public interface IManagerOverlayService
    {
        Task<bool> RequestApprovalAsync(string promptTitle, bool reasonRequired = false);

        /// <summary>
        /// Shows the PIN overlay and returns whether the manager approved the
        /// action (plus the optional reason they entered). <see cref="ManagerApprovalResult.Approved"/>
        /// is false on cancel, on failed/locked PIN entry, or when the manager
        /// has no PIN set. Callers must check <c>Approved</c> before proceeding.
        /// </summary>
        Task<ManagerApprovalResult> RequestApprovalWithReasonAsync(string promptTitle);
    }
}
