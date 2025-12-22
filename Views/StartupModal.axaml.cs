using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks; 


namespace vengar.Views;

public partial class StartupModal : Window
{
    public static TaskCompletionSource<bool?> CompletionSource { get; private set; }

    public StartupModal()
    {
        InitializeComponent();
        CompletionSource = new TaskCompletionSource<bool?>();
    }

    public void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        CompletionSource.SetResult(true);
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!CompletionSource.Task.IsCompleted)
        {
            CompletionSource.SetResult(false);
        }
        base.OnClosing(e);
    }
}