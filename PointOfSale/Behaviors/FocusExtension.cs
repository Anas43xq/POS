using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Behaviors
{
    /// <summary>
    /// Attached behavior to set keyboard focus to the first item in an ItemsControl
    /// </summary>
    public static class FocusExtension
    {
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
                        System.Windows.Threading.DispatcherPriority.Background,
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
    }
}
