using System.Collections;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using vengar.Models;
using vengar.ViewModels;

namespace vengar.Views;

public partial class DnsView : UserControl
{
    public DnsView()
    {
        InitializeComponent();
    }
    

    private async Task CopyColumnAsync(Func<DnsRecordEntry, string> selector)
    {
        if (DnsDataGrid.SelectedItems is not IList selected || selected.Count == 0)
            return;

        var sb = new StringBuilder();
        foreach (var item in selected)
        {
            if (item is DnsRecordEntry r)
                sb.AppendLine(selector(r));
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            await topLevel.Clipboard.SetTextAsync(sb.ToString().TrimEnd());

    }

    private async void CopyType_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await CopyColumnAsync(r => r.Type);

    private async void CopyName_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await CopyColumnAsync(r => r.Name);

    private async void CopyValue_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await CopyColumnAsync(r => r.Value);

    private async void CopyTtl_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await CopyColumnAsync(r => r.Ttl.ToString());

}