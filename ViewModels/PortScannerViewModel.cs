using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using vengar.Data;
using vengar.Interfaces;
using vengar.Models;

namespace vengar.ViewModels;

public partial class PortScannerViewModel : PageViewModel
{
    private readonly IPortScanner _scanner;
    private readonly IWriter _writer;

    [ObservableProperty] private string _host = "";
    [ObservableProperty] private string _portsText = "";
    [ObservableProperty] private PortPreset? _selectedPreset;
    [ObservableProperty] private string _scanStatus = "Idle (stopped)";
    [ObservableProperty] private bool _isScanning;
    

    public ObservableCollection<PortPreset> Presets { get; } = new();
    public ObservableCollection<PortScanEntry> Results { get; } = new();

    public PortScannerViewModel(IPortScanner scanner, IWriter writer)
        : base("Port Scanner", AppPageNames.PortScanner)
    {
        _scanner = scanner;
        _writer = writer;

        Presets.Add(new PortPreset
        {
            Name = "Application Ports",
            Ports = "515,631,3282,3389,5190,5050,4443,1863,6891,1503,5631,5632,5900,6667"
        });

        Presets.Add(new PortPreset
        {
            Name = "Server Ports",
            Ports = "21,22,23,25,53,80,110,137,138,139,143,443,445,548,587,993,995,1433,1701,1723,3306,5432,8008,8443"
        });

        Presets.Add(new PortPreset
        {
            Name = "Custom Ports",
            Ports = ""
        });
    }

    public PortScannerViewModel()
    {
        // design-time
    }

    partial void OnSelectedPresetChanged(PortPreset? value)
    {
        if (value != null)
            PortsText = value.Ports;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        Results.Clear();

        if (string.IsNullOrWhiteSpace(Host))
            return;

        var ports = ParsePorts(PortsText).Distinct().Order().ToList();
        if (ports.Count == 0)
            return;

        IsScanning = true;
        ScanStatus = "Starting scan...";

        _writer.Write($"[PORTSCAN] Host={Host}");
        _writer.Write($"[PORTSCAN] Ports={string.Join(",", ports)}");

        try
        {
            foreach (var port in ports)
            {
                ScanStatus = $"Scanning port {port}...";
                await Task.Yield(); // forces UI refresh

                // Use your existing service ScanAsync for a single port
                var result = (await _scanner.ScanAsync(_writer, Host, new[] { port })).FirstOrDefault();
                if (result != null)
                    Results.Add(result);
            }

            ScanStatus = $"Scan completed ({Results.Count} ports)";
            _writer.Write($"[PORTSCAN] Scan completed ({Results.Count} ports)");
        }
        catch (Exception ex)
        {
            ScanStatus = "Scan failed";
            _writer.Write($"[PORTSCAN][ERROR] {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }

    private static IEnumerable<int> ParsePorts(string text)
    {
        return text.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => int.TryParse(p.Trim(), out var port) ? port : -1)
            .Where(p => p > 0 && p <= 65535);
    }
    

    [RelayCommand]
    private void SaveLogs(Visual visual)
    {
        _writer.ExportLogsAsync(visual, "portscan-logs");
    }

    [RelayCommand]
    private void ClearLogs()
    {
        _writer.ClearLogs();
    }

}

public class PortStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            PortScanStatus.Open     => Brushes.LightGreen,
            PortScanStatus.TimedOut => Brushes.Gold,
            PortScanStatus.Closed   => Brushes.Orange,
            PortScanStatus.Error    => Brushes.IndianRed,
            _ => Brushes.Transparent
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
