using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using vengar.Data;

namespace vengar.ViewModels;

public partial class HomeViewModel : PageViewModel
{
    
    public string AppName { get; } = "Vengar";
    public string AppDescription { get; } = "Network Utility Tools";
    

    public HomeViewModel() : base("Home", AppPageNames.Home)
    {
        PageName = AppPageNames.Home;
    }
    
}