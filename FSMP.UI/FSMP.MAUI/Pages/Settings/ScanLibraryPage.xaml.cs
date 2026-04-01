using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages.Settings;

public partial class ScanLibraryPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly IServiceScope _scope;

    public ScanLibraryPage()
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
        catch (Exception ex) { App.Log($"ScanLibraryPage.OnAppearing error: {ex}"); }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        NavOverlay.CurrentRoute = "Settings";
        NavOverlay.Toggle();
    }

    private void OnScanCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is SettingsViewModel.DirectoryItem item)
        {
            var isSelected = _viewModel.SelectedScanPaths.Contains(item.Path);
            if (e.Value != isSelected)
                _viewModel.ToggleScanPathCommand.Execute(item.Path);
        }
    }
}
