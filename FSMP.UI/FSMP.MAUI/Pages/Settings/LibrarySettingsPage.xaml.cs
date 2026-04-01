using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages.Settings;

public partial class LibrarySettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly IServiceScope _scope;

    public LibrarySettingsPage()
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
        catch (Exception ex) { App.Log($"LibrarySettingsPage.OnAppearing error: {ex}"); }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnManageDirectoriesClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("manageDirectories");

    private async void OnScanLibraryClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("scanLibrary");

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        NavOverlay.CurrentRoute = "Settings";
        NavOverlay.Toggle();
    }
}
