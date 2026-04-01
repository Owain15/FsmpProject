using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages.Settings;

public partial class AboutSettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly IServiceScope _scope;

    public AboutSettingsPage()
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
        catch (Exception ex) { App.Log($"AboutSettingsPage.OnAppearing error: {ex}"); }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        NavOverlay.CurrentRoute = "Settings";
        NavOverlay.Toggle();
    }

    private void OnDirectoriesDataTapped(object? sender, TappedEventArgs e)
    {
        _viewModel.ToggleDirectoriesDataCommand.Execute(null);
        DirectoriesDataHeader.Text = _viewModel.IsDirectoriesDataExpanded
            ? "Directories Data \u25BC"
            : "Directories Data \u25B6";
    }

    private void OnToggleAllDirectoriesClicked(object? sender, EventArgs e)
    {
        _viewModel.ToggleAllDirectoriesCommand.Execute(null);
        ToggleAllButton.Text = _viewModel.AreAllDirectoriesExpanded ? "Collapse All" : "Expand All";
    }

    private void OnDirectoryTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is SettingsViewModel.DirectoryStatsItem item)
            _viewModel.ToggleDirectoryCommand.Execute(item);
    }
}
