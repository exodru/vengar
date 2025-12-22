using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using vengar.ViewModels;
using vengar.Views;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using vengar.Factories;
using vengar.Interfaces;
using vengar.Services;

namespace vengar;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        // this.AttachDevTools();
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection(); 
        collection.AddSingleton<MainWindowViewModel>();
        
        collection.AddTransient<IWriter, FileWriter>();
        collection.AddTransient<IPingService, PingService>();
        collection.AddTransient<IDnsLookup, DnsLookupService>();
        collection.AddSingleton<IPortScanner, PortScannerService>();
        collection.AddSingleton<IIpToolsService, IpToolsService>();

        
        collection.AddSingleton<HomeViewModel>();
        collection.AddTransient<PingViewModel>();
        collection.AddTransient<DnsViewModel>();
        collection.AddTransient<PortScannerViewModel>();
        collection.AddTransient<IpToolsViewModel>();


        // collection.AddSingleton<Func<AppPageNames, PageViewModel>>(servProv => appName => appName switch
        // {
        //     AppPageNames.Home => servProv.GetRequiredService<HomeViewModel>(),
        //     AppPageNames.Ping => servProv.GetRequiredService<PingViewModel>(),
        //     _ => throw new ArgumentOutOfRangeException(nameof(appName), appName, null)
        // });
        
        collection.AddSingleton<PageFactory>();
        
        var serviceProvider = collection.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // var modal = new StartupModal();
            // modal.Show();
            // bool? result = await StartupModal.CompletionSource.Task;
            bool result = true;
            
            if (result == true)
            {
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow { DataContext = serviceProvider.GetService<MainWindowViewModel>() };
                desktop.MainWindow.Show();
                // modal.Close();
            }
            else
            {
                // modal.Close();
                desktop.Shutdown();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    
    
 
}