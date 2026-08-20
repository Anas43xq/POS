namespace UI.Services
{
    /// <summary>
    /// Visual/semantic category for a toast notification. Drives the
    /// color used by <c>ToastHost</c> (see Controls/ToastHost.xaml).
    /// </summary>
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
