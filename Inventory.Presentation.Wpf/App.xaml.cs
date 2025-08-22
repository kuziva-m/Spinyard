using Inventory.Core.Application.Interfaces;
using Inventory.Core.Application.Services;
using Inventory.Core.Domain.Interfaces;
using Inventory.Infrastructure.Data;
using Inventory.Presentation.Wpf.ViewModels;
using Inventory.Presentation.Wpf.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace Inventory.Presentation.Wpf
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // --- FIX 1: Change DbContext and UnitOfWork to Transient ---
            // This ensures every operation gets a fresh context, preventing collisions.
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql("Host=aws-1-ap-south-1.pooler.supabase.com; " +
                                   "Database=postgres; " +
                                   "Username=postgres.tksonejmooqlovsxvvda; " +
                                   "Password=SpinyardDatabase; " +
                                   "Port=5432; " + // <-- Note the port is 5432
                                   "SSL Mode=Require; " +
                                   "Trust Server Certificate=true"), ServiceLifetime.Transient);

            services.AddTransient<IUnitOfWork, UnitOfWork>();

            // This can stay scoped to the operation that uses it
            services.AddScoped<IInventoryService, InventoryService>();

            // Services
            services.AddSingleton<IDialogService, Services.DialogService>();

            // ViewModels - Singleton so they maintain state
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<InventoryViewModel>();
            services.AddSingleton<MainViewModel>();

            // Windows/Dialogs - Transient because they are created and destroyed
            services.AddTransient<AddEditItemViewModel>();
            services.AddTransient<AddEditItemWindow>();
            services.AddTransient<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                // Apply any pending migrations to the database
                await dbContext.Database.MigrateAsync();
            }

            // --- FIX 2: Properly initialize the view model ---
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            await mainViewModel.InitializeAsync(); // We will create this method next

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel; // Explicitly set the DataContext
            mainWindow.Show();

            base.OnStartup(e);
        }

        // ... OnExit method ...
    }
}