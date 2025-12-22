using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using vengar.Factories;

namespace vengar.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly PageFactory _pageFactory;
    public string AppName { get; } = "Vengar";
    [ObservableProperty] private bool _sideMenuExpanded = true;
    [ObservableProperty] private PageViewModel? _currentPage;
    public ObservableCollection<PageViewModel> Tabs { get; } = new();
    [ObservableProperty] private PageViewModel? _selectedTab;
    private const string TabsSaveFile = "tabs.json";

    public MainWindowViewModel(PageFactory pageFactory)
    {
        _pageFactory = pageFactory;
        GoHomePage();
    }
    
    public MainWindowViewModel()
    {
        // DESIGN-TIME ONLY
    }


    [RelayCommand]
    private void ExpandSidebar() => SideMenuExpanded = !SideMenuExpanded;

    [RelayCommand]
    private void GoHomePage()
    {
        var existingHome = Tabs.OfType<HomeViewModel>().FirstOrDefault();
        if (existingHome != null)
        {
            SelectedTab = existingHome;
            return;
        }

        var vm = _pageFactory.CreatePage<HomeViewModel>();
        vm.Title = "Home"; 
        Console.WriteLine($"GoHomePage fired with title: {vm.Title}");
        Tabs.Add(vm);
        SelectedTab = vm;
    }

    [RelayCommand]
    private void GoPingPage()
    {
        // CurrentPage = _pageFactory.GetPage(AppPageNames.Ping);
        var vm = _pageFactory.CreatePage<PingViewModel>();
        int number = Tabs.Count(t => t is PingViewModel) + 1;
        vm.Title = $"Ping #{number}";
        Console.WriteLine($"GoPingPage fired with title: {vm.Title}");
        Tabs.Add(vm);
        SelectedTab = vm;
    }
    
    [RelayCommand]
    private void GoDnsLookupPage()
    {
        var vm = _pageFactory.CreatePage<DnsViewModel>();
        int number = Tabs.Count(t => t is DnsViewModel) + 1;
        vm.Title = $"DNS Lookup #{number}";
        Console.WriteLine($"GoDnsLookupPage fired with title: {vm.Title}");
        Tabs.Add(vm);
        SelectedTab = vm;
    }
    
    
    [RelayCommand]
    private void GoPortScannerPage()
    {
        var vm = _pageFactory.CreatePage<PortScannerViewModel>();
        int number = Tabs.Count(t => t is PortScannerViewModel) + 1;
        vm.Title = $"Port Scanner #{number}";
        Console.WriteLine($"GoPortScannerPage fired with title: {vm.Title}");
        Tabs.Add(vm);
        SelectedTab = vm;
    }
    
    [RelayCommand]
    private void GoIpToolsPage()
    {
        var vm = _pageFactory.CreatePage<IpToolsViewModel>();
        int number = Tabs.Count(t => t is IpToolsViewModel) + 1;
        vm.Title = $"IP Tools #{number}";
        Console.WriteLine($"GoIpToolsPage fired with title: {vm.Title}");
        Tabs.Add(vm);
        SelectedTab = vm;
    }

    [RelayCommand]
    private void CloseTab(PageViewModel? tab)
    {
        if (tab is null) return;
        Tabs.Remove(tab);
        if (SelectedTab == tab) SelectedTab = Tabs.LastOrDefault();
    }
    
        [RelayCommand]
        private void Quit()
        {
            if (Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
}