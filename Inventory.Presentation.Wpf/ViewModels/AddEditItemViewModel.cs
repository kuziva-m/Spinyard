using Inventory.Core.Application.DTOs;
using Inventory.Core.Application.Interfaces;
using Inventory.Presentation.Wpf.Commands;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Inventory.Presentation.Wpf.ViewModels
{
    public class ProductVariantViewModel : ViewModelBase
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string OptionNames { get; set; } = string.Empty;
        public string OptionValues { get; set; } = string.Empty;
    }

    public class ProductOptionViewModel : ViewModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Values { get; set; } = string.Empty;
    }

    public class AddEditItemViewModel : ViewModelBase
    {
        private static readonly string[] OptionSplitSeparator = [" / "];

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

        public ObservableCollection<CategoryDto> Categories { get; } = [];
        public ObservableCollection<ProductVariantViewModel> Variants { get; } = [];
        public ObservableCollection<ProductOptionViewModel> Options { get; } = [];

        private CategoryDto? _selectedCategory;
        public CategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

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

        private string _variantOptionsHeader = "Variant";
        public string VariantOptionsHeader
        {
            get => _variantOptionsHeader;
            set { _variantOptionsHeader = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand AddOptionCommand { get; }
        public ICommand RemoveOptionCommand { get; }
        public ICommand GenerateVariantsCommand { get; }
        public Action? CloseWindow { get; set; }

        public string WindowTitle => IsEditing ? "Edit Item" : "Add Item";
        public bool HasOptions => Options.Count > 0;
        public bool HasVariants => Variants.Count > 0;

        public AddEditItemViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            Options.CollectionChanged += (s, e) => UpdateVariantOptionsHeader();

            SaveCommand = new RelayCommand(async _ => await SaveItem(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => CloseWindow?.Invoke());
            BrowseImageCommand = new RelayCommand(_ => BrowseImage());
            AddOptionCommand = new RelayCommand(_ => AddOption(), _ => !string.IsNullOrWhiteSpace(NewOptionName) && !string.IsNullOrWhiteSpace(NewOptionValues));

            // ✅ FIX 1: Corrected the typo from "Relay-Command" to "RelayCommand"
            RemoveOptionCommand = new RelayCommand(option => RemoveOption(option));
            GenerateVariantsCommand = new RelayCommand(_ => GenerateVariants());

            _ = LoadCategoriesAsync();
            UpdateVariantOptionsHeader();
        }

        private async Task LoadCategoriesAsync()
        {
            // ✅ FIX 2: Added a try-catch block to prevent crashes if the database is unavailable.
            try
            {
                var categories = await _inventoryService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load categories in Add/Edit window: {ex.Message}");
                // The window will still open, but the category dropdown will be empty.
            }
        }

        // --- No other changes are needed below this line ---

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

        private void UpdateVariantOptionsHeader()
        {
            if (Options.Count > 0)
            {
                VariantOptionsHeader = string.Join(" / ", Options.Select(o => o.Name));
            }
            else
            {
                VariantOptionsHeader = "Variant";
            }
        }

        private void GenerateVariants()
        {
            if (Options.Count == 0 || Options.Any(o => string.IsNullOrWhiteSpace(o.Values)))
                return;

            var optionValueLists = Options
                .Select(o => o.Values
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList())
                .ToList();

            if (optionValueLists.Count == 0)
                return;

            var optionNamesStr = string.Join(" / ", Options.Select(o => o.Name));
            var existingVariants = Variants.ToDictionary(v => v.OptionValues, v => v);
            Variants.Clear();

            var combinations = GetCartesianProduct(optionValueLists);

            foreach (var combo in combinations)
            {
                var optionValuesStr = string.Join(" / ", combo);
                ProductVariantViewModel? bestMatch = null;

                foreach (var existingVariant in existingVariants.Values)
                {
                    var existingOptions = existingVariant.OptionValues.Split(OptionSplitSeparator, StringSplitOptions.None).ToList();
                    if (existingOptions.All(opt => combo.Contains(opt)))
                    {
                        if (bestMatch == null || existingOptions.Count > bestMatch.OptionValues.Split(OptionSplitSeparator, StringSplitOptions.None).ToList().Count)
                        {
                            bestMatch = existingVariant;
                        }
                    }
                }

                if (bestMatch != null)
                {
                    Variants.Add(new ProductVariantViewModel
                    {
                        OptionNames = optionNamesStr,
                        OptionValues = optionValuesStr,
                        Sku = bestMatch.Sku,
                        Quantity = bestMatch.Quantity,
                        Price = bestMatch.Price
                    });
                    existingVariants.Remove(bestMatch.OptionValues);
                }
                else
                {
                    Variants.Add(new ProductVariantViewModel
                    {
                        OptionNames = optionNamesStr,
                        OptionValues = optionValuesStr,
                        Quantity = 0,
                        Price = 0.0m
                    });
                }
            }
            OnPropertyChanged(nameof(HasVariants));
        }

        private static System.Collections.Generic.List<System.Collections.Generic.List<string>> GetCartesianProduct(System.Collections.Generic.List<System.Collections.Generic.List<string>> sequences)
        {
            var result = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
            if (sequences.Count == 0)
            {
                result.Add([]);
                return result;
            }

            var firstSequence = sequences.First();
            var remainingSequences = sequences.Skip(1).ToList();
            var remainingCombinations = GetCartesianProduct(remainingSequences);

            foreach (var item in firstSequence)
            {
                foreach (var combo in remainingCombinations)
                {
                    var newCombo = new System.Collections.Generic.List<string> { item };
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
            SelectedCategory = Categories.FirstOrDefault(c => c.Name == product.CategoryName);

            var optionNames = product.OptionNames?.Split(',').Select(n => n.Trim()).ToList() ?? [];
            var optionNamesStr = string.Join(" / ", optionNames);

            Variants.Clear();
            foreach (var variant in product.Variants)
            {
                var optionValuesStr = string.Join(" / ", variant.Variation);
                Variants.Add(new ProductVariantViewModel
                {
                    Sku = variant.Sku,
                    Quantity = variant.Quantity,
                    Price = variant.Price,
                    OptionNames = optionNamesStr,
                    OptionValues = optionValuesStr
                });
            }
            OnPropertyChanged(nameof(HasVariants));

            Options.Clear();
            if (!string.IsNullOrEmpty(product.OptionNames))
            {
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
            UpdateVariantOptionsHeader();
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
                    Variants = [.. Variants.Select(static v => new ProductVariantCreateDto
                        {
                            Sku = v.Sku,
                            Price = v.Price,
                            Quantity = v.Quantity,
                            Variation = [.. v.OptionValues.Split(OptionSplitSeparator, StringSplitOptions.None)]
                        })],
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
                    Variants = [.. Variants.Select(v => new ProductVariantDto
                    {
                        Sku = v.Sku,
                        Quantity = v.Quantity,
                        Price = v.Price,
                        Variation = [.. v.OptionValues.Split(OptionSplitSeparator, StringSplitOptions.None)]
                    })],
                    OptionNames = optionNames
                };
                await _inventoryService.UpdateProductAsync(updateProductDto);
            }
            CloseWindow?.Invoke();
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(ProductName) && SelectedCategory != null;

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