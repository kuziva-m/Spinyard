using Inventory.Presentation.Wpf.Commands;
using System.Windows.Input;

namespace Inventory.Presentation.Wpf.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        // These commands will be assigned by the MainViewModel
        public ICommand? NavigateToInventoryCommand { get; set; }
        public ICommand? AddNewProductCommand { get; set; }
        public ICommand? NavigateToSettingsCommand { get; set; }

        public DashboardViewModel()
        {
            // The constructor is now empty because the MainViewModel 
            // is responsible for creating and assigning the commands.
        }
    }
}