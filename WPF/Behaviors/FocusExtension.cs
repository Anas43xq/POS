using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace UI.Behaviors
{
    /// <summary>
    /// Attached behaviors for setting keyboard focus from XAML without code-behind.
    /// FocusFirstItem targets generated item templates; IsFocused focuses an input
    /// element on a false-to-true transition and then ignores later false updates.
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
                    itemsControl.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            var firstContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
                            if (firstContainer is ContentPresenter contentPresenter)
                            {
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
            if (d is UIElement element && (bool)e.NewValue)
            {
                element.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    new Action(() =>
                    {
                        element.Focusable = true;
                        element.Focus();
                        if (element is TextBox textBox)
                            textBox.SelectAll();
                    }));
            }
        }
    }
}
