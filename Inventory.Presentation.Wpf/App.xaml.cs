using Inventory.Core.Application.Interfaces;
using Inventory.Core.Application.Services;
using Inventory.Core.Domain.Entities;
using Inventory.Core.Domain.Interfaces;
using Inventory.Infrastructure.Data;
using Inventory.Presentation.Wpf.ViewModels;
using Inventory.Presentation.Wpf.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using DomainAttribute = Inventory.Core.Domain.Entities.Attribute;

namespace Inventory.Presentation.Wpf
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public static string WritableDatabasePath { get; private set; } = string.Empty;

        public App()
        {
            // Define a persistent, writable location for the database in the user's AppData folder.
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataFolder, "InventoryManagementSystem");
            Directory.CreateDirectory(appFolder); // Ensure the folder exists.
            WritableDatabasePath = Path.Combine(appFolder, "inventory.db");

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure DbContext to always use the writable database file path.
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlite($"Data Source={WritableDatabasePath}"));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddSingleton<Inventory.Core.Application.Interfaces.IDialogService, Inventory.Presentation.Wpf.Services.DialogService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<AddEditItemViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<AddEditItemWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Debug.WriteLine($"Database Path: {Path.GetFullPath("inventory.db")}");
            await _host.StartAsync();


            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

                // If the database doesn't exist at the writable path, create it.
                if (!File.Exists(WritableDatabasePath))
                {
                    await CreateAndSeedDatabaseAsync(dbContext);
                }
                else
                {
                    // If it exists, just ensure migrations are applied.
                    await dbContext.Database.MigrateAsync();
                }
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private async Task CreateAndSeedDatabaseAsync(InventoryDbContext dbContext)
        {
            // The resource name is "DefaultNamespace.FileName". Check your project properties if needed.
            var resourceName = "Inventory.Presentation.Wpf.inventory.db";
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream? resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream != null)
                {
                    // If the embedded resource exists, copy it.
                    using (FileStream fileStream = new FileStream(WritableDatabasePath, FileMode.CreateNew))
                    {
                        await resourceStream.CopyToAsync(fileStream);
                    }
                    // Apply migrations to the newly extracted DB.
                    await dbContext.Database.MigrateAsync();
                }
                else
                {
                    // If no embedded DB is found, create a new one from migrations and seed it.
                    await dbContext.Database.MigrateAsync();
                    await SeedDataAsync(dbContext);
                }
            }
        }

        private async Task SeedDataAsync(InventoryDbContext dbContext)
        {
            if (await dbContext.Products.AnyAsync()) return; // Don't seed if data already exists.

            // 1. Create and save Attributes first
            var typeAttr = new DomainAttribute { Name = "Type" };
            var colorAttr = new DomainAttribute { Name = "Color" };
            var sizeAttr = new DomainAttribute { Name = "Size" };
            var flavorAttr = new DomainAttribute { Name = "Flavor" };
            dbContext.Attributes.AddRange(typeAttr, colorAttr, sizeAttr, flavorAttr);
            await dbContext.SaveChangesAsync();

            // 2. Create and save Attribute Options
            var standardOption = new AttributeOption { Attribute = typeAttr, Value = "Standard" };
            var greyOption = new AttributeOption { Attribute = colorAttr, Value = "Grey" };
            var blackOption = new AttributeOption { Attribute = colorAttr, Value = "Black" };
            var whiteOption = new AttributeOption { Attribute = colorAttr, Value = "White" };
            var blueOption = new AttributeOption { Attribute = colorAttr, Value = "Blue" };
            var redOption = new AttributeOption { Attribute = colorAttr, Value = "Red" };
            var mediumOption = new AttributeOption { Attribute = sizeAttr, Value = "M" };
            var lavOption = new AttributeOption { Attribute = flavorAttr, Value = "Lavender" };
            var lemOption = new AttributeOption { Attribute = flavorAttr, Value = "Lemon" };
            dbContext.AttributeOptions.AddRange(standardOption, greyOption, blackOption, whiteOption, blueOption, redOption, mediumOption, lavOption, lemOption);
            await dbContext.SaveChangesAsync();

            // 3. Create and save Categories
            var electronics = new Category { Name = "Electronics" };
            var apparel = new Category { Name = "Apparel" };
            var office = new Category { Name = "Office Supplies" };
            var toiletries = new Category { Name = "Toiletries" };
            dbContext.Categories.AddRange(electronics, apparel, office, toiletries);
            await dbContext.SaveChangesAsync();

            // 4. Create Products and link everything together
            // Product 1: Laptop
            var product1 = new Product { Name = "Laptop Pro 15-inch", CategoryId = electronics.Id, OptionNames = "Type,Color" };
            var p1v1 = new ProductVariant { SKU = "LP-PRO-15-GRY", Quantity = 15, Price = 1299.99m };
            p1v1.AttributeOptions.Add(standardOption);
            p1v1.AttributeOptions.Add(greyOption);
            product1.Variants.Add(p1v1);

            // Product 2: Wireless Mouse
            var product2 = new Product { Name = "Wireless Mouse", CategoryId = electronics.Id, OptionNames = "Color" };
            var p2v1 = new ProductVariant { SKU = "WM-LOG-BLK", Quantity = 150, Price = 25.50m };
            p2v1.AttributeOptions.Add(blackOption);
            var p2v2 = new ProductVariant { SKU = "WM-LOG-WHT", Quantity = 120, Price = 25.50m };
            p2v2.AttributeOptions.Add(whiteOption);
            product2.Variants.Add(p2v1);
            product2.Variants.Add(p2v2);

            // Product 3: Men's T-Shirt
            var product3 = new Product { Name = "Men's T-Shirt", CategoryId = apparel.Id, OptionNames = "Color,Size" };
            var p3v1 = new ProductVariant { SKU = "TSHIRT-M-BLU", Quantity = 75, Price = 19.99m };
            p3v1.AttributeOptions.Add(blueOption);
            p3v1.AttributeOptions.Add(mediumOption);
            var p3v2 = new ProductVariant { SKU = "TSHIRT-M-RED", Quantity = 80, Price = 19.99m };
            p3v2.AttributeOptions.Add(redOption);
            p3v2.AttributeOptions.Add(mediumOption);
            product3.Variants.Add(p3v1);
            product3.Variants.Add(p3v2);

            // Product 4: Ballpoint Pens
            var product4 = new Product { Name = "Ballpoint Pens (12-Pack)", CategoryId = office.Id, OptionNames = "Type" };
            var p4v1 = new ProductVariant { SKU = "PEN-BP-12PK", Quantity = 200, Price = 8.99m };
            p4v1.AttributeOptions.Add(standardOption);
            product4.Variants.Add(p4v1);

            // Product 5: Hand Soap
            var product5 = new Product { Name = "Hand Soap", CategoryId = toiletries.Id, OptionNames = "Flavor" };
            var p5v1 = new ProductVariant { SKU = "HS-LAV", Quantity = 50, Price = 2.50m };
            p5v1.AttributeOptions.Add(lavOption);
            var p5v2 = new ProductVariant { SKU = "HS-LEM", Quantity = 45, Price = 2.50m };
            p5v2.AttributeOptions.Add(lemOption);
            product5.Variants.Add(p5v1);
            product5.Variants.Add(p5v2);

            // 5. Add all products to the context and save
            dbContext.Products.AddRange(product1, product2, product3, product4, product5);
            await dbContext.SaveChangesAsync();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}
