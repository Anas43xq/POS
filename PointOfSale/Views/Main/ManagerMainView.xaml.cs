using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UI.Configuration;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ManagerMainView : UserControl
    {
        private readonly ShortcutSettings _shortcuts = App.ServiceProvider.GetRequiredService<ShortcutSettings>();

        public ManagerMainView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ManagerMainViewModel vm)
                return;

            var k = _shortcuts.Manager;

            // Navigation shortcuts
            BindKey(k.Home, vm.NavigateHomeCommand);
            BindKey(k.Products, vm.NavigateProductManagementCommand);
            BindKey(k.Categories, vm.NavigateCategoryManagementCommand);
            BindKey(k.Sizes, vm.NavigateSizeManagementCommand);
            BindKey(k.Transactions, vm.NavigateTransactionsCommand);
            BindKey(k.Reports, vm.NavigateReportsCommand);
            BindKey(k.ReceiptManagement, vm.NavigateReceiptManagementCommand);
            BindKey(k.ShiftManagement, vm.NavigateShiftManagementCommand);
            BindKey(k.Settings, vm.ShowSetting);
        }

        private void BindKey(string keyGesture, ICommand command)
        {
            if (TryParseKeyGesture(keyGesture, out var key, out var modifiers))
                InputBindings.Add(new KeyBinding(command, key, modifiers));
        }

        private static bool TryParseKeyGesture(string gesture, out Key key, out ModifierKeys modifiers)
        {
            key = Key.None;
            modifiers = ModifierKeys.None;

            if (string.IsNullOrWhiteSpace(gesture))
                return false;

            var parts = gesture.Split('+');
            var keyPart = parts[^1].Trim();

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var mod = parts[i].Trim().ToLowerInvariant();
                modifiers |= mod switch
                {
                    "ctrl" or "control" => ModifierKeys.Control,
                    "shift" => ModifierKeys.Shift,
                    "alt" => ModifierKeys.Alt,
                    _ => ModifierKeys.None
                };
            }

            return Enum.TryParse<Key>(keyPart, ignoreCase: true, out key) && key != Key.None;
        }
    }
}
