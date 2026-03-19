using FSMP.MAUI.ViewModels;

namespace FSMP.MAUI.Pages;

public partial class CustomThemePage : ContentPage
{
    private readonly CustomThemeViewModel _viewModel;

    public CustomThemePage(CustomThemeViewModel viewModel)
    {
        _viewModel = viewModel;
        Resources.Add("BoolToHighlightConverter", new BoolToHighlightConverter());
        InitializeComponent();
        BindingContext = _viewModel;

        _viewModel.PromptForName = async (message) =>
            await DisplayPromptAsync("Save Theme", message);

        _viewModel.ConfirmDelete = async (message) =>
            await DisplayAlertAsync("Delete Theme", message, "Delete", "Cancel");

        SizeChanged += (_, _) => _viewModel.IsWideLayout = Width >= 600;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.IsWideLayout = Width >= 600;
        await _viewModel.LoadAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

/// <summary>
/// Converts bool IsSelected to a highlight background or transparent.
/// </summary>
public class BoolToHighlightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is true)
        {
            if (Application.Current?.Resources.TryGetValue("ThemeHighlight", out var res) == true && res is Color c)
                return c;
            return Colors.LightBlue;
        }
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}
