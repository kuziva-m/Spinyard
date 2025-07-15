using Inventory.Core.Application.DTOs;

namespace Inventory.Core.Application.Interfaces
{
    public interface IDialogService
    {
        void ShowAddEditItemWindow(ProductDto? productToEdit = null);
    }
}
