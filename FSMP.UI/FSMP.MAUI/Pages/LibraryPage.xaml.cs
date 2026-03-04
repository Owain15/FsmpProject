using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryBrowseViewModel _viewModel;
    private readonly IServiceScope _scope;

    public LibraryPage()
    {
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<LibraryBrowseViewModel>();
        InitializeComponent();
        BindingContext = _viewModel;
        Unloaded += (_, _) => _scope.Dispose();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            App.Log($"LibraryPage.OnAppearing error: {ex}");
        }
    }
}
