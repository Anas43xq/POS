using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace UI.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _isDisposed;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            string nameToRaise = propName ?? string.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameToRaise));
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
