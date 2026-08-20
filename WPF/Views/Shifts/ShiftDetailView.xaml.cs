using System.Windows;
using System.Windows.Input;

namespace UI.Views
{
    public partial class ShiftDetailView : Window
    {
        public ShiftDetailView()
        {
            InitializeComponent();
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
        }
    }
}
