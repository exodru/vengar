using CommunityToolkit.Mvvm.ComponentModel;
using vengar.Data;

namespace vengar.ViewModels;

public abstract partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private AppPageNames _pageName;

    protected PageViewModel(string title, AppPageNames pageName)
    {
        Title = title;
        PageName = pageName;
    }
    
    public PageViewModel()
    {
        Title = "Design";
        PageName = AppPageNames.Unknown;
    }

}
