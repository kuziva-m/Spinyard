using Inventory.Core.Application.DTOs;
using Inventory.Core.Application.Interfaces;
using Inventory.Presentation.Wpf.Commands;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Inventory.Presentation.Wpf.ViewModels
{
    public class ProductVariantViewModel : ViewModelBase
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Variation { get; set; } = string.Empty;
    }

    public class ProductOptionViewModel : ViewModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Values { get; set; } = string.Empty;
    }

    public class AddEditItemViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private int _productId;
        private string _productName = "";
        public string ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(); }
        }

        private string? _imagePath;
        public string? ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CategoryDto> Categories { get; }

        private CategoryDto? _selectedCategory;
        public CategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductVariantViewModel> Variants { get; }
        public ObservableCollection<ProductOptionViewModel> Options { get; }

        private string _newOptionName = string.Empty;
        public string NewOptionName
        {
            get => _newOptionName;
            set { _newOptionName = value; OnPropertyChanged(); }
        }

        private string _newOptionValues = string.Empty;
        public string NewOptionValues
        {
            get => _newOptionValues;
            set { _newOptionValues = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand AddOptionCommand { get; }
        public ICommand RemoveOptionCommand { get; }
        public ICommand GenerateVariantsCommand { get; }
        public Action? CloseWindow { get; set; }

        public string WindowTitle => IsEditing ? "Edit Item" : "Add Item";
        public bool HasOptions => Options.Any();

        public AddEditItemViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            Categories = new ObservableCollection<CategoryDto>();
            Variants = new ObservableCollection<ProductVariantViewModel>();
            Options = new ObservableCollection<ProductOptionViewModel>();

            SaveCommand = new RelayCommand(async _ => await SaveItem(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => CloseWindow?.Invoke());
            BrowseImageCommand = new RelayCommand(_ => BrowseImage());
            AddOptionCommand = new RelayCommand(_ => AddOption(), _ => !string.IsNullOrWhiteSpace(NewOptionName) && !string.IsNullOrWhiteSpace(NewOptionValues));
            RemoveOptionCommand = new RelayCommand(option => RemoveOption(option));
            GenerateVariantsCommand = new RelayCommand(_ => GenerateVariants());

            _ = LoadCategoriesAsync();
        }

        private void AddOption()
        {
            Options.Add(new ProductOptionViewModel { Name = NewOptionName, Values = NewOptionValues });
            NewOptionName = string.Empty;
            NewOptionValues = string.Empty;
            OnPropertyChanged(nameof(HasOptions));
        }

        private void RemoveOption(object? option)
        {
            if (option is ProductOptionViewModel optionToRemove)
            {
                Options.Remove(optionToRemove);
                OnPropertyChanged(nameof(HasOptions));
            }
        }
        private void GenerateVariants()
        {
            if (!Options.Any() || Options.Any(o => string.IsNullOrWhiteSpace(o.Values)))
                return;

            var optionValueLists = Options
                .Select(o => o.Values
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList())
                .ToList();

            if (optionValueLists.Any(list => !list.Any()))
                return;

            var existingVariants = Variants.ToDictionary(v => v.Variation, v => v);
            Variants.Clear();

            var combinations = GetCartesianProduct(optionValueLists);

            foreach (var combo in combinations)
            {
                var variation = string.Join(" / ", combo);
                if (existingVariants.TryGetValue(variation, out var existing))
                {
                    Variants.Add(existing);
                }
                else
                {
                    Variants.Add(new ProductVariantViewModel
                    {
                        Variation = variation,
                        Quantity = 0,
                        Price = 0.0m
                    });
                }
            }
        }

        private List<List<string>> GetCartesianProduct(List<List<string>> sequences)
        {
            var result = new List<List<string>>();
            if (!sequences.Any())
            {
                result.Add(new List<string>());
                return result;
            }

            var firstSequence = sequences.First();
            var remainingSequences = sequences.Skip(1).ToList();
            var remainingCombinations = GetCartesianProduct(remainingSequences);

            foreach (var item in firstSequence)
            {
                foreach (var combo in remainingCombinations)
                {
                    var newCombo = new List<string> { item };
                    newCombo.AddRange(combo);
                    result.Add(newCombo);
                }
            }
            return result;
        }

        public void LoadProductForEditing(ProductDto product)
        {
            _productId = product.Id;
            IsEditing = true;
            ProductName = product.Name;
            ImagePath = product.Variants.FirstOrDefault()?.ImagePath;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.Id);

            Variants.Clear();
            foreach (var variant in product.Variants)
            {
                Variants.Add(new ProductVariantViewModel
                {
                    Sku = variant.Sku,
                    Quantity = variant.Quantity,
                    Price = variant.Price,
                    Variation = string.Join(" / ", variant.Variation)
                });
            }

            Options.Clear();
            if (product.OptionNames != null && product.OptionNames.Any())
            {
                var optionNames = product.OptionNames.Split(',').Select(n => n.Trim()).ToList();
                var allVariantsOptions = product.Variants.Select(v => v.Variation).ToList();

                for (int i = 0; i < optionNames.Count; i++)
                {
                    var optionValues = allVariantsOptions
                        .Where(v => v.Count > i)
                        .Select(v => v[i])
                        .Distinct()
                        .ToList();

                    Options.Add(new ProductOptionViewModel
                    {
                        Name = optionNames[i],
                        Values = string.Join(", ", optionValues)
                    });
                }
            }
            OnPropertyChanged(nameof(HasOptions));
        }

        private async Task SaveItem()
        {
            if (SelectedCategory == null) return;
            var optionNames = string.Join(",", Options.Select(o => o.Name));

            if (_productId == 0)
            {
                var newProductDto = new ProductCreateDto
                {
                    ProductName = this.ProductName,
                    CategoryId = this.SelectedCategory.Id,
                    ImagePath = this.ImagePath,
                    Variants = new List<ProductVariantCreateDto>(
                        Variants.Select(v => new ProductVariantCreateDto
                        {
                            Sku = v.Sku,
                            Price = v.Price,
                            Quantity = v.Quantity,
                            Variation = v.Variation.Split(new[] { " / " }, StringSplitOptions.None).ToList()
                        })),
                    OptionNames = optionNames
                };
                await _inventoryService.CreateProductAsync(newProductDto);
            }
            else
            {
                var updateProductDto = new ProductUpdateDto
                {
                    Id = _productId,
                    ProductName = this.ProductName,
                    CategoryId = this.SelectedCategory.Id,
                    ImagePath = this.ImagePath,
                    Variants = new List<ProductVariantDto>(
                    Variants.Select(v => new ProductVariantDto
                    {
                        Sku = v.Sku,
                        Quantity = v.Quantity,
                        Price = v.Price,
                        Variation = v.Variation.Split(new[] { " / " }, StringSplitOptions.None).ToList()
                    })),
                    OptionNames = optionNames
                };
                await _inventoryService.UpdateProductAsync(updateProductDto);
            }
            CloseWindow?.Invoke();
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(ProductName) && SelectedCategory != null;

        private async Task LoadCategoriesAsync()
        {
            var categories = await _inventoryService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private void BrowseImage()
        {
            var dialog = new OpenFileDialog { Title = "Select an Image", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
            if (dialog.ShowDialog() == true)
            {
                ImagePath = dialog.FileName;
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }
    }
}