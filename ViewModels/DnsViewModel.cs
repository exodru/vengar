using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using vengar.Helpers;
using vengar.Interfaces;
using vengar.Models;
using Avalonia.Media;
using vengar.Data;

namespace vengar.ViewModels;

public partial class DnsViewModel : PageViewModel
{
    private IWriter _writer;
    private readonly IDnsLookup _dnsService;
    [ObservableProperty] private string _hostname = "";
    [ObservableProperty] private ObservableCollection<DnsRecordEntry> _records = [];
    [ObservableProperty] private ObservableCollection<DnsRecordEntry> _selectedRecords = new();

    public DnsViewModel(IDnsLookup dnsService, IWriter writer) : base("DNS Lookup", AppPageNames.DnsLookup)
    {
        _writer = writer;
        _dnsService = dnsService;
    }
    public DnsViewModel()
    {
        // Design-time only
        Records = new ObservableCollection<DnsRecordEntry>();
    }

    [RelayCommand]
    private async Task LookupAsync()
    {
        Records.Clear();
        var result = await _dnsService.LookupAsync(_writer, Hostname);
        if (result.Success)
            foreach (var record in result.Records)
                Records.Add(record);
    }

    [RelayCommand]
    private async Task SaveLogs(Visual visual)
    {
        await _writer.ExportLogsAsync(visual, "dns_logs");
    }

    [RelayCommand]
    private void ClearLogs()
    {
        _writer.ClearLogs();
    }

    [RelayCommand]
    private async Task CopySelected(object? parameter)
    {
        if (parameter is not IList<DnsRecordEntry> selected || selected.Count == 0)
            return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Type\tName\tValue\tTTL");

        foreach (var r in selected)
            sb.AppendLine($"{r.Type}\t{r.Name}\t{r.Value}\t{r.Ttl}");

        // Clipboard via TopLevel
        var top = Avalonia.Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (top != null)
        {
            await top.Clipboard.SetTextAsync(sb.ToString().TrimEnd());
        }

        _writer.Write($"[COPY] {selected.Count} record(s) copied to clipboard");
    }
}

public class DnsTypeTooltipConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DnsRecordHelp.Get(value?.ToString() ?? "");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class DnsRecordTypeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = value?.ToString() switch
        {
            "A" => Colors.DodgerBlue,
            "AAAA" => Colors.Orange,
            "MX" => Colors.DarkGreen,
            "CNAME" => Colors.Goldenrod,
            "NS" => Colors.DarkSlateBlue,
            "TXT" => Colors.MidnightBlue,
            "SOA" => Colors.DarkRed,
            _ => Colors.LightSlateGray
        };
        return new SolidColorBrush(color);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}