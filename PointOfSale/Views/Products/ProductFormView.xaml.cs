using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UI.ViewModels;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for ProductFormView.xaml
    /// </summary>
    public partial class ProductFormView : Window
    {
        public ProductFormView()
        {
            InitializeComponent();
            Loaded += (_, __) =>
            {
                if (DataContext is ProductFormViewModel vm)
                {
                    vm.DialogClosed += () =>
                    {
                        DialogResult = false;
                        Close();
                    };
                }
            };
        }

        private void OverlayGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == OverlayGrid)
            {
                DialogResult = false;  
                Close();
            }
        }

        private void DialogCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ProductFormView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
