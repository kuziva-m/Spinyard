Spinyard Inventory Management System

Spinyard is a modern, high-performance desktop inventory management system built on .NET 8 and WPF. It demonstrates a clean, decoupled architecture suitable for robust and scalable enterprise applications.

This application provides a clean interface for managing products, their categories, and complex product variants (such as size, color, or material) with real-time search and filtering.

🚀 Features

    Full Product Management (CRUD): Create, read, update, and delete products.

    Dynamic Product Variants: Define options for a product (e.g., "Size," "Color") and automatically generate all possible variants (e.g., "Small, Red," "Medium, Blue").

    Category Management (CRUD): Organize products by custom categories.

    Powerful Search & Filtering: Instantly search products by name/SKU or filter by category.

    Modern UI: A clean, responsive, and intuitive user interface built with Material Design in XAML.

    Persistent Storage: All data is saved to a robust PostgreSQL database.

🛠️ Tech Stack & Architecture

This project is built using a decoupled, 4-layer Clean Architecture to separate concerns, making it maintainable, testable, and scalable.

Core Technologies

    .NET 8: The latest long-term support (LTS) version of the .NET platform.

    WPF (Windows Presentation Foundation): A powerful UI framework for building desktop applications.

    Entity Framework Core 8: The ORM used to map C# objects to the database.

    PostgreSQL: A powerful, open-source object-relational database system.

    Material Design in XAML: A popular library for bringing Google's Material Design principles to WPF applications.

    GitHub Actions: Used for Continuous Integration (CI) to build and publish the application on every push.

Design Patterns & Principles

    Clean Architecture: The solution is separated into four distinct projects:

        Core.Domain: Contains only the business entities and repository interfaces.

        Core.Application: Contains business logic, services, and DTOs.

        Infrastructure: Implements data access (repositories, DbContext) and other external services.

        Presentation.Wpf: The WPF user interface.

    MVVM (Model-View-ViewModel): The standard design pattern for WPF, separating UI (View) from logic (ViewModel).

    Repository & Unit of Work: Abstracts data access into a manageable, testable pattern.

    Dependency Injection (DI): The .NET generic host is used to manage and inject dependencies (like services and repositories) throughout the application, promoting loose coupling.

🏗️ Solution Architecture

/InventoryManagementSystem.sln
│
├── 1. Inventory.Core.Domain/
│   ├── Entities/         (Product.cs, Category.cs, ProductVariant.cs)
│   └── Interfaces/       (IProductRepository.cs, IUnitOfWork.cs)
│
├── 2. Inventory.Core.Application/
│   ├── DTOs/             (ProductDto.cs, CategoryDto.cs)
│   ├── Interfaces/       (IInventoryService.cs, IDialogService.cs)
│   └── Services/         (InventoryService.cs)
│
├── 3. Inventory.Infrastructure/
│   ├── Data/             (InventoryDbContext.cs, UnitOfWork.cs)
│   ├── Repositories/     (ProductRepository.cs, CategoryRepository.cs)
│   └── Migrations/
│
└── 4. Inventory.Presentation.Wpf/
    ├── Views/            (InventoryView.xaml, DashboardView.xaml)
    ├── ViewModels/       (InventoryViewModel.cs, DashboardViewModel.cs)
    ├── Services/         (DialogService.cs)
    └── App.xaml.cs       (Dependency Injection setup)

🏁 How to Run

Prerequisites

    .NET 8.0 SDK

    A running PostgreSQL database instance (local or cloud-hosted, e.g., on Supabase or Railway).

Configuration

    Clone the repository:
    Bash

git clone https://github.com/your-username/your-repository-name.git
cd your-repository-name

Set the Connection String: Open the file Inventory.Infrastructure/Data/InventoryDbContextFactory.cs. Update the connectionString variable with your own PostgreSQL connection string.
C#

// In Inventory.Infrastructure/Data/InventoryDbContextFactory.cs
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        // *** REPLACE THIS WITH YOUR CONNECTION STRING ***
        var connectionString = "Host=...; Database=...; Username=...; Password=...;";

        optionsBuilder.UseNpgsql(connectionString);
        return new InventoryDbContext(optionsBuilder.Options);
    }
}

Update the runtime connection string: Do the same in Inventory.Presentation.Wpf/App.xaml.cs.
C#

// In Inventory.Presentation.Wpf/App.xaml.cs
private static void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<InventoryDbContext>(options =>
        // *** REPLACE THIS WITH YOUR CONNECTION STRING ***
        options.UseNpgsql("Host=...; Database=...; Username=...; Password=...;"), 
        ServiceLifetime.Transient);

    // ... other services
}

Run the application: The application will automatically run the database migrations on its first launch to create all necessary tables.
Bash

    # Navigate to the WPF project
    cd Inventory.Presentation.Wpf

    # Run the application
    dotnet run

📈 Future Improvements

    Authentication & Authorization: Add user login and roles (Admin, User).

    Reporting: A dedicated view for generating reports on stock levels and sales.

    Low-Stock Alerts: Automatically flag items that fall below a certain quantity.

    Dashboard Analytics: Add charts and KPIs to the dashboard.
