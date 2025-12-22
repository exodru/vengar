using vengar.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace vengar.Factories;

public class PageFactory()
{
    // public PageViewModel GetPage(AppPageNames pageName) => factory.Invoke(pageName);
    private readonly IServiceProvider _serviceProvider;

    public PageFactory(IServiceProvider serviceProvider) : this()
    {
        _serviceProvider = serviceProvider;
    }

    public T CreatePage<T>() where T : PageViewModel
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}