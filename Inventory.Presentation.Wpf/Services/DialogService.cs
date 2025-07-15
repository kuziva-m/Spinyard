using Inventory.Core.Application.DTOs;
using Inventory.Core.Application.Interfaces;
using Inventory.Presentation.Wpf.ViewModels;
using Inventory.Presentation.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Presentation.Wpf.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowAddEditItemWindow(ProductDto? productToEdit = null)
        {
            var window = _serviceProvider.GetRequiredService<AddEditItemWindow>();
            var viewModel = _serviceProvider.GetRequiredService<AddEditItemViewModel>();

            // If we are editing, call a method on the ViewModel to load the product data
            if (productToEdit != null)
            {
                viewModel.LoadProductForEditing(productToEdit);
            }

            viewModel.CloseWindow = window.Close;
            window.DataContext = viewModel;
            window.ShowDialog();
        }
    }
}