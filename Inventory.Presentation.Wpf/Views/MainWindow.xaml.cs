// In Inventory.Presentation.Wpf/Views/MainWindow.xaml.cs
using Inventory.Presentation.Wpf.ViewModels;
using System.Windows;

namespace Inventory.Presentation.Wpf.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}