using Inventory.Core.Application.DTOs;
using Inventory.Core.Application.Interfaces;
using Inventory.Presentation.Wpf.Commands;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Inventory.Presentation.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private string _categoryName = string.Empty;
        private bool _isFormVisible;
        private bool _isEditing;
        private CategoryDto? _selectedCategory;

        public ObservableCollection<CategoryDto> Categories { get; } = [];

        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set { _isFormVisible = value; OnPropertyChanged(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        public CategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand ShowAddFormCommand { get; }
        public ICommand ShowEditFormCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LoadCategoriesCommand { get; }
        public ICommand? NavigateToDashboardCommand { get; set; }

        public SettingsViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;

            LoadCategoriesCommand = new RelayCommand(async _ => await LoadCategories());
            ShowAddFormCommand = new RelayCommand(_ => ShowForm(isEditing: false));
            ShowEditFormCommand = new RelayCommand(_ => ShowForm(isEditing: true), _ => SelectedCategory != null);
            CancelCommand = new RelayCommand(_ => HideForm());
            SaveCommand = new RelayCommand(async _ => await SaveCategory(), _ => !string.IsNullOrWhiteSpace(CategoryName));
            DeleteCommand = new RelayCommand(async _ => await DeleteCategory(), _ => SelectedCategory != null);
        }

        public async Task InitializeAsync()
        {
            HideForm();
            await LoadCategories();
        }

        private async Task LoadCategories()
        {
            try
            {
                var categories = await _inventoryService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Could not load categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowForm(bool isEditing)
        {
            IsEditing = isEditing;
            if (IsEditing && SelectedCategory != null)
            {
                CategoryName = SelectedCategory.Name;
            }
            else
            {
                CategoryName = string.Empty;
            }
            IsFormVisible = true;
        }

        private void HideForm()
        {
            IsFormVisible = false;
            IsEditing = false;
            CategoryName = string.Empty;
        }

        private async Task SaveCategory()
        {
            try
            {
                if (IsEditing && SelectedCategory != null)
                {
                    var dto = new CategoryUpdateDto { Id = SelectedCategory.Id, Name = CategoryName };
                    await _inventoryService.UpdateCategoryAsync(dto);
                }
                else
                {
                    var dto = new CategoryCreateDto { Name = CategoryName };
                    await _inventoryService.AddCategoryAsync(dto);
                }
                HideForm();
                await LoadCategories();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to save category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCategory()
        {
            if (SelectedCategory == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the category '{SelectedCategory.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _inventoryService.DeleteCategoryAsync(SelectedCategory.Id);
                    await LoadCategories();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}