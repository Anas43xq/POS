namespace UI.Services
{
    /// <summary>
    /// Shows the manager PIN overlay and returns true if the manager
    /// approved the action, false if they cancelled or failed.
    /// </summary>
    public interface IManagerOverlayService
    {
        Task<bool> RequestApprovalAsync(string promptTitle, bool reasonRequired = false);

        Task<string?> RequestApprovalWithReasonAsync(string promptTitle);
    }
}
