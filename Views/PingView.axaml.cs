using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using vengar.Data;
using vengar.Interfaces;

namespace vengar.Views;

public partial class PingView : UserControl
{
    public PingView()
    {
        InitializeComponent();
    }
    
    private async Task CopyPingColumnAsync(Func<PingEntry, string?> selector)
    {
        if (PingDataGrid.SelectedItems is not IList selected || selected.Count == 0)
            return;

        var sb = new StringBuilder();

        foreach (var item in selected)
        {
            if (item is PingEntry p)
            {
                var value = selector(p);
                if (!string.IsNullOrWhiteSpace(value))
                    sb.AppendLine(value);
            }
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            await topLevel.Clipboard.SetTextAsync(sb.ToString().TrimEnd());
    }
    
    private async void CopyPingHost_OnClick(object? sender, RoutedEventArgs e)
        => await CopyPingColumnAsync(p => p.Hostname);

    private async void CopyPingAddress_OnClick(object? sender, RoutedEventArgs e)
        => await CopyPingColumnAsync(p => p.Address);


}