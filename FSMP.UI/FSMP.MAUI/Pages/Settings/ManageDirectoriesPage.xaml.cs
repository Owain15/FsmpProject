using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages.Settings;

public partial class ManageDirectoriesPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly IServiceScope _scope;

    public ManageDirectoriesPage()
    {
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        BindingContext = _viewModel;
        Unloaded += (_, _) => _scope.Dispose();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _viewModel.LoadAsync(); }
        catch (Exception ex) { App.Log($"ManageDirectoriesPage.OnAppearing error: {ex}"); }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        NavOverlay.CurrentRoute = "Settings";
        NavOverlay.Toggle();
    }
}
