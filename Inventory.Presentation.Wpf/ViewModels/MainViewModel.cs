using Inventory.Presentation.Wpf.Commands;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Inventory.Presentation.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        private readonly InventoryViewModel _inventoryViewModel;

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            InventoryViewModel inventoryViewModel)
        {
            _inventoryViewModel = inventoryViewModel;

            // --- FIX: Initialize the field directly to satisfy the compiler ---
            _currentViewModel = dashboardViewModel;

            // Set up the commands that the Dashboard will use
            dashboardViewModel.NavigateToInventoryCommand = new RelayCommand(_ => CurrentViewModel = _inventoryViewModel);
            dashboardViewModel.NavigateToSettingsCommand = new RelayCommand(_ => { /* Future logic for settings */ });
            dashboardViewModel.AddNewProductCommand = _inventoryViewModel.AddItemCommand;
            inventoryViewModel.NavigateToDashboardCommand = new RelayCommand(_ => CurrentViewModel = dashboardViewModel);
        }

        /// <summary>
        /// This method is called from App.xaml.cs to safely load initial data.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _inventoryViewModel.InitializeAsync();
        }
    }
}