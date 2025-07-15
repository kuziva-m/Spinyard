using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inventory.Presentation.Wpf.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // The '?' makes the event nullable, matching the interface contract
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}