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
        {
            if (theme == "Custom")
                _ = ApplyCustomThemeFromConfigAsync();
            else
                ThemeManager.ApplyTheme(theme);
        }
    }

    private async Task ApplyCustomThemeFromConfigAsync()
    {
        try
        {
            var configService = _scope.ServiceProvider.GetRequiredService<Core.Interfaces.IConfigurationService>();
            var config = await configService.LoadConfigurationAsync();
            if (config.CustomThemeColors is not null && config.CustomThemeColors.Count > 0)
                ThemeManager.ApplyCustomTheme(config.CustomThemeColors);
            else
                ThemeManager.ApplyTheme("Light");
        }
        catch (Exception ex)
        {
            App.Log($"Failed to apply custom theme: {ex}");
        }
    }

    private async void OnCustomizeThemeClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("customTheme");

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
