using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ManagerMainView : UserControl
    {
        private Dictionary<ManagerPageId, RadioButton>? _navMap;
        private static readonly Duration SlideDuration = new(TimeSpan.FromMilliseconds(180));

        public ManagerMainView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _navMap = new Dictionary<ManagerPageId, RadioButton>
            {
                { ManagerPageId.Home,               NavHome },
                { ManagerPageId.Transactions,       NavTransactions },
                { ManagerPageId.ShiftManagement,    NavShift },
                { ManagerPageId.Reports,            NavReports },
                { ManagerPageId.Products,           NavProducts },
                { ManagerPageId.Categories,         NavCategories },
                { ManagerPageId.Sizes,              NavSizes },
                { ManagerPageId.ReceiptManagement,  NavReceipts },
                { ManagerPageId.ModifierGroups,     NavModifiers },
            };

            if (DataContext is ManagerMainViewModel vm)
            {
                vm.PropertyChanged += OnVmPropertyChanged;
                PlaceHighlightAt(vm.ActivePage, animate: false);
            }
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ManagerMainViewModel.ActivePage)) return;
            if (sender is ManagerMainViewModel vm)
                PlaceHighlightAt(vm.ActivePage, animate: true);
        }

        private void PlaceHighlightAt(ManagerPageId page, bool animate)
        {
            if (_navMap is null || !_navMap.TryGetValue(page, out var btn))
                return;

            btn.UpdateLayout();
            SidebarHighlight.UpdateLayout();

            double itemHeight = btn.ActualHeight;
            if (itemHeight <= 0) itemHeight = (double)FindResource("Size.TouchTarget.Min");
            SidebarHighlight.Height = itemHeight;
            SidebarHighlight.Width = NavPanel.ActualWidth > 0 ? NavPanel.ActualWidth : 180;

            var transform = btn.TransformToVisual(SidebarHighlight.Parent as UIElement ?? (UIElement)NavPanel);
            double targetY = transform.Transform(new Point(0, 0)).Y;

            if (animate)
            {
                var anim = new DoubleAnimation(targetY, SlideDuration)
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                    FillBehavior = FillBehavior.HoldEnd,
                };
                HighlightTranslate.BeginAnimation(TranslateTransform.YProperty, anim);
            }
            else
            {
                HighlightTranslate.BeginAnimation(TranslateTransform.YProperty, null);
                HighlightTranslate.Y = targetY;
            }
        }
    }
}
