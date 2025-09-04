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
        private readonly SettingsViewModel _settingsViewModel;

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            InventoryViewModel inventoryViewModel,
            SettingsViewModel settingsViewModel)
        {
            _inventoryViewModel = inventoryViewModel;
            _currentViewModel = dashboardViewModel;
            _settingsViewModel = settingsViewModel;

            // Set up the commands that the Dashboard will use
            dashboardViewModel.NavigateToInventoryCommand = new RelayCommand(_ => CurrentViewModel = _inventoryViewModel);
            dashboardViewModel.NavigateToSettingsCommand = new RelayCommand(async _ =>
            {
                await _settingsViewModel.InitializeAsync(); // Load categories
                CurrentViewModel = _settingsViewModel;
            });
            dashboardViewModel.AddNewProductCommand = _inventoryViewModel.AddItemCommand;
            inventoryViewModel.NavigateToDashboardCommand = new RelayCommand(_ => CurrentViewModel = dashboardViewModel);
            settingsViewModel.NavigateToDashboardCommand = new RelayCommand(_ => CurrentViewModel = dashboardViewModel);
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