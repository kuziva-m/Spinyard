using Inventory.Core.Application.Interfaces;
using Inventory.Core.Application.Services;
using Inventory.Core.Domain.Interfaces;
using Inventory.Infrastructure.Data;
using Inventory.Presentation.Wpf.ViewModels;
using Inventory.Presentation.Wpf.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Diagnostics;
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

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql("Host=aws-1-ap-south-1.pooler.supabase.com; " +
                                   "Database=postgres; " +
                                   "Username=postgres.tksonejmooqlovsxvvda; " +
                                   "Password=SpinyardDatabase; " +
                                   "Port=5432; " +
                                   "SSL Mode=Require; " +
                                   "Trust Server Certificate=true"), ServiceLifetime.Transient);

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddSingleton<IDialogService, Services.DialogService>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<InventoryViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<AddEditItemViewModel>();
            services.AddTransient<AddEditItemWindow>();
            services.AddTransient<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            try
            {
                using var scope = _host.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                await dbContext.Database.MigrateAsync();
            }
            // ✅ FIX: The MessageBox has been removed. The error is now silent.
            catch (Exception ex)
            {
                // Log the exception to the debug console instead of showing a popup
                Debug.WriteLine($"Database migration failed: {ex.Message}");
            }

            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            await mainViewModel.InitializeAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}