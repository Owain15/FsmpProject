using FSMP.Core.ViewModels;
using FSMP.MAUI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages.Settings;

public partial class AppearanceSettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly IServiceScope _scope;

    public AppearanceSettingsPage()
    {
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        BindingContext = _viewModel;
        ThemePicker.SelectedIndexChanged += OnThemePickerChanged;
        Unloaded += (_, _) => _scope.Dispose();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _viewModel.LoadAsync(); }
        catch (Exception ex) { App.Log($"AppearanceSettingsPage.OnAppearing error: {ex}"); }
    }

    private void OnThemePickerChanged(object? sender, EventArgs e)
    {
        if (ThemePicker.SelectedItem is string theme)
            ThemeManager.ApplyTheme(theme);
    }

    private void OnTextSizePickerChanged(object? sender, EventArgs e)
    {
        if (TextSizePicker.SelectedItem is string size)
            TextSizeManager.ApplyTextSize(size);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        NavOverlay.CurrentRoute = "Settings";
        NavOverlay.Toggle();
    }
}
