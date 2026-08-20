using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BLL.Models;
using UI.Services;

namespace UI.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Optional notification service used by <see cref="RunAsync{T}"/>
        /// to surface <see cref="Result{T}.Error"/> messages. Subclasses
        /// that use <see cref="RunAsync{T}"/> should set this in their
        /// constructor (typically from an injected
        /// <see cref="INotificationService"/>).
        /// </summary>
        protected INotificationService? Notifications { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            string nameToRaise = propName ?? string.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameToRaise));
        }

        /// <summary>
        /// Runs an operation that returns a <see cref="Result{T}"/>,
        /// invoking <paramref name="onSuccess"/> with the value on success
        /// and surfacing <c>result.Error</c> via
        /// <see cref="INotificationService.ShowError"/> on failure.
        /// Unexpected exceptions are not caught here — they propagate to
        /// the app's global unhandled-exception handler.
        /// </summary>
        protected async Task RunAsync<T>(
            Func<Task<Result<T>>> operation,
            Func<T, Task> onSuccess)
        {
            var result = await operation();
            if (result.IsSuccess)
            {
                await onSuccess(result.Value!);
            }
            else
            {
                Notifications?.ShowError(result.Error ?? "An error occurred.");
            }
        }

        /// <summary>
        /// Releases resources held by this ViewModel — most importantly,
        /// unsubscribing from any singleton-owned events (e.g.
        /// <c>ILocalizationService.LanguageChanged</c>) so a transient
        /// ViewModel instance can be garbage-collected instead of being
        /// permanently rooted by the singleton publisher.
        /// <para>
        /// Derived classes should override <see cref="Dispose(bool)"/>,
        /// not this method, and must call the base implementation.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override to unsubscribe from events, dispose owned child
        /// ViewModels, or release other resources. Called at most once;
        /// guarded by <see cref="_isDisposed"/> in the base class.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
        }
    }
}