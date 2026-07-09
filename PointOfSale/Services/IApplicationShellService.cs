using System.Windows;

namespace UI.Services
{
    /// <summary>
    /// Owns the application shell lifecycle so <c>App.xaml.cs</c>
    /// can stay focused on DI composition and startup wiring.
    ///
    /// Responsibilities:
    /// • Show the Login-As window on startup.
    /// • On a successful Manager / Cashier login, swap the
    ///   Login-As window out for the MainWindow.
    /// • On logout, swap the MainWindow out for a fresh
    ///   Login-As window.
    /// </summary>
    public interface IApplicationShellService
    {
        /// <summary>Starts the application: shows the Login-As window.</summary>
        void Start();

        /// <summary>
        /// Shows the main application window and closes the
        /// supplied <paramref name="loginAsWindow"/>. Wires the
        /// MainWindow's logout event so a subsequent logout
        /// returns to a fresh Login-As window.
        /// </summary>
        void OpenMainWindow(Window loginAsWindow);
    }
}
