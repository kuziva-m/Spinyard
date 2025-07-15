using Inventory.Core.Application.DTOs;
using Inventory.Core.Application.Interfaces;
using Inventory.Presentation.Wpf.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Inventory.Presentation.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<ProductDto> Products { get; }
        private ProductDto? _selectedProduct;

        private string? _searchTerm;
        public string? SearchTerm { get => _searchTerm; set { _searchTerm = value; OnPropertyChanged(); } }
        public ObservableCollection<CategoryDto> FilterCategories { get; }
        private CategoryDto? _selectedFilterCategory;
        public CategoryDto? SelectedFilterCategory { get => _selectedFilterCategory; set { _selectedFilterCategory = value; OnPropertyChanged(); } }

        public ProductDto? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadAllInventoryCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public MainViewModel(IInventoryService inventoryService, IDialogService dialogService)
        {
            _inventoryService = inventoryService;
            _dialogService = dialogService;
            Products = new ObservableCollection<ProductDto>();
            FilterCategories = new ObservableCollection<CategoryDto>();

            LoadAllInventoryCommand = new RelayCommand(async _ => await LoadAllInventory());
            AddItemCommand = new RelayCommand(async _ => await OpenAddItemWindow());
            EditItemCommand = new RelayCommand(async _ => await OpenEditItemWindow(), _ => SelectedProduct != null);
            DeleteItemCommand = new RelayCommand(async _ => await DeleteSelectedItemAsync(), _ => SelectedProduct != null);
            SearchCommand = new RelayCommand(async _ => await LoadProducts());
            ClearFilterCommand = new RelayCommand(async _ => await ClearFiltersAndLoad());

            _ = LoadFilterCategoriesAsync();
            _ = LoadAllInventory();
        }

        private async Task LoadFilterCategoriesAsync()
        {
            var categories = await _inventoryService.GetCategoriesAsync();
            FilterCategories.Clear();
            FilterCategories.Add(new CategoryDto { Id = 0, Name = "All Categories" });
            foreach (var category in categories)
            {
                FilterCategories.Add(category);
            }
        }

        private async Task OpenAddItemWindow()
        {
            _dialogService.ShowAddEditItemWindow();
            await LoadProducts();
        }

        private async Task OpenEditItemWindow()
        {
            if (SelectedProduct == null) return;
            _dialogService.ShowAddEditItemWindow(SelectedProduct);
            await LoadProducts();
        }

        private async Task DeleteSelectedItemAsync()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedProduct.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _inventoryService.DeleteProductAsync(SelectedProduct.Id);
                await LoadProducts();
            }
        }

        private async Task LoadProducts()
        {
            int? categoryId = (SelectedFilterCategory == null || SelectedFilterCategory.Id == 0) ? null : SelectedFilterCategory.Id;
            var products = await _inventoryService.SearchProductsAsync(SearchTerm, categoryId);

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private async Task LoadAllInventory()
        {
            var products = await _inventoryService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private async Task ClearFiltersAndLoad()
        {
            SearchTerm = string.Empty;
            SelectedFilterCategory = null;
            await LoadAllInventory();
        }
    }
}