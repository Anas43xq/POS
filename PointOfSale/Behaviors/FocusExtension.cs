using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace UI.Behaviors
{
    /// <summary>
    /// Attached behaviors for setting keyboard focus from XAML /
    /// ViewModels without code-behind.
    ///
    /// Two attached dependency properties are exposed:
    ///
    /// • <c>FocusFirstItem</c> — for an <see cref="ItemsControl"/>,
    ///   focuses the first focusable descendant after the items
    ///   have been generated.
    ///
    /// • <c>IsFocused</c> — for any <see cref="IInputElement"/> (e.g.
    ///   <see cref="TextBox"/>, <see cref="PasswordBox"/>), focuses
    ///   the element when the bound value transitions to <c>true</c>.
    ///   Subsequent transitions back to <c>false</c> are ignored so
    ///   the user can keep typing without the VM stealing focus.
    /// </summary>
    public static class FocusExtension
    {
        // ─────────────────────────────────────────────────────────
        //  FocusFirstItem  (existing, for ItemsControl)
        // ─────────────────────────────────────────────────────────

        public static readonly DependencyProperty FocusFirstItemProperty =
            DependencyProperty.RegisterAttached(
                "FocusFirstItem",
                typeof(bool),
                typeof(FocusExtension),
                new PropertyMetadata(false, OnFocusFirstItemChanged));

        public static bool GetFocusFirstItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusFirstItemProperty);
        }

        public static void SetFocusFirstItem(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusFirstItemProperty, value);
        }

        private static void OnFocusFirstItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl && (bool)e.NewValue)
            {
                itemsControl.Loaded += (sender, args) =>
                {
                    // Wait for the items to be generated
                    itemsControl.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            var firstContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
                            if (firstContainer is ContentPresenter contentPresenter)
                            {
                                // Find the first Focusable element (Button) in the template
                                var focusableChild = FindFocusableElement(contentPresenter);
                                focusableChild?.Focus();
                            }
                        }));
                };
            }
        }

        private static IInputElement? FindFocusableElement(DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is Button button && button.Focusable)
                {
                    return button;
                }

                var descendant = FindFocusableElement(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }

        // ─────────────────────────────────────────────────────────
        //  IsFocused  (new — for TextBox / PasswordBox / etc.)
        // ─────────────────────────────────────────────────────────

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(FocusExtension),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Only act on the false→true transition. We don't want the VM
            // to yank focus away from the user on every property update.
            if (d is UIElement element && (bool)e.NewValue)
            {
                // The element may not yet be in the visual tree when the
                // bound value flips true during construction. Defer one
                // dispatcher pass and focus after layout has had a chance
                // to attach the element.
                element.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    new Action(() =>
                    {
                        element.Focusable = true;
                        element.Focus();
                        // Select-all for editable text surfaces is a nice
                        // touch if a remembered username is being re-shown.
                        if (element is TextBox textBox)
                            textBox.SelectAll();
                    }));
            }
        }
    }
}
