using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using vengar.Data;
using vengar.Models;
using vengar.Services;

namespace vengar.ViewModels;

public partial class IpToolsViewModel : PageViewModel
{
    [ObservableProperty] private string _input = "";
    [ObservableProperty] private int _selectedCidr = 24;
    [ObservableProperty] private string _startIp = "";
    [ObservableProperty] private string _endIp = "";
    [ObservableProperty] private IpInfo _calculatedInfo = new();
    [ObservableProperty] private ObservableCollection<string> _range = new();
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _ptrResult = "";

    

    public List<int> CidrOptions { get; } = new() { 8, 12, 16, 24 };

    private readonly IIpToolsService _service;

    public IpToolsViewModel(IIpToolsService service) : base("IP Tools", AppPageNames.IpTools)
    {
        _service = service;
    }

    public IpToolsViewModel()
    {
        _range = new ObservableCollection<string>();
        _calculatedInfo = new IpInfo();
        SelectedCidr = 24;
    }



    [RelayCommand]
    private void Calculate()
    {
        if (string.IsNullOrWhiteSpace(Input)) return;

        CalculatedInfo = _service.Calculate(Input, SelectedCidr);

        // Automatically fill start/end fields for generation
        var parts = CalculatedInfo.Range.Split(" - ");
        if (parts.Length == 2)
        {
            StartIp = parts[0];
            EndIp = parts[1];
        }

        PtrResult = "";
        StatusMessage = $"Calculated {Input}/{SelectedCidr}: {CalculatedInfo.Range}";
        Range.Clear(); // clear previous range
    }

    [RelayCommand]
    private void GenerateRange()
    {
        Range.Clear();
        foreach (var ip in _service.GenerateRange(StartIp, EndIp))
            Range.Add(ip);

        StatusMessage = $"Generated {Range.Count} IP(s) from {StartIp} to {EndIp}";
    }

    [RelayCommand]
    private void DetectPrivate()
    {
        CalculatedInfo.IsPrivate = _service.IsPrivate(Input);
    }

    [RelayCommand]
    private async Task CopyRangeAsync()
    {
        var text = string.Join(Environment.NewLine, Range);
        if (Avalonia.Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            var top = desktop.MainWindow;
            if (top != null)
                await top.Clipboard.SetTextAsync(text);
        }

        StatusMessage = $"Copied {Range.Count} IP(s) to clipboard";
    }
    
    [RelayCommand]
    private async Task ResolvePtrAsync()
    {
        PtrResult = "Resolving...";
        var result = await _service.ResolvePtrAsync(Input);
        PtrResult = result ?? "No PTR record found";

        StatusMessage = "PTR lookup completed";
    }

}
