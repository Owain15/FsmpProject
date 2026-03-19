using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FSMP.MAUI.Helpers;

namespace FSMP.MAUI.ViewModels;

public class PreviewColorDictionary : INotifyPropertyChanged
{
    private readonly Dictionary<string, Color> _colors = new();

    public Color this[string key]
    {
        get => _colors.TryGetValue(key, out var c) ? c : Colors.Transparent;
        set { _colors[key] = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]")); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class ColorEntry : INotifyPropertyChanged
{
    private string _hexValue = "#FFFFFF";
    private bool _isSelected;

    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;

    public string HexValue
    {
        get => _hexValue;
        set
        {
            if (_hexValue == value) return;
            _hexValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ColorPreview));
            OnPropertyChanged(nameof(Red));
            OnPropertyChanged(nameof(Green));
            OnPropertyChanged(nameof(Blue));
            OnPropertyChanged(nameof(Hue));
            OnPropertyChanged(nameof(Saturation));
            OnPropertyChanged(nameof(Brightness));
        }
    }

    public Color ColorPreview
    {
        get
        {
            try { return Color.FromArgb(HexValue); }
            catch { return Colors.White; }
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected == value) return; _isSelected = value; OnPropertyChanged(); }
    }

    public int Red
    {
        get => ColorPreview.Red >= 0 ? (int)(ColorPreview.Red * 255) : 0;
        set { UpdateFromRgb(value, Green, Blue); }
    }

    public int Green
    {
        get => ColorPreview.Green >= 0 ? (int)(ColorPreview.Green * 255) : 0;
        set { UpdateFromRgb(Red, value, Blue); }
    }

    public int Blue
    {
        get => ColorPreview.Blue >= 0 ? (int)(ColorPreview.Blue * 255) : 0;
        set { UpdateFromRgb(Red, Green, value); }
    }

    private bool _updatingFromHsb;

    public double Hue
    {
        get { RgbToHsb(Red, Green, Blue, out var h, out _, out _); return h; }
        set { if (!_updatingFromHsb) UpdateFromHsb(value, Saturation, Brightness); }
    }

    public double Saturation
    {
        get { RgbToHsb(Red, Green, Blue, out _, out var s, out _); return s; }
        set { if (!_updatingFromHsb) UpdateFromHsb(Hue, value, Brightness); }
    }

    public double Brightness
    {
        get { RgbToHsb(Red, Green, Blue, out _, out _, out var b); return b; }
        set { if (!_updatingFromHsb) UpdateFromHsb(Hue, Saturation, value); }
    }

    private void UpdateFromHsb(double h, double s, double b)
    {
        _updatingFromHsb = true;
        try
        {
            HsbToRgb(h, s, b, out var r, out var g, out var bl);
            HexValue = $"#{r:X2}{g:X2}{bl:X2}";
            OnPropertyChanged(nameof(Hue));
            OnPropertyChanged(nameof(Saturation));
            OnPropertyChanged(nameof(Brightness));
        }
        finally { _updatingFromHsb = false; }
    }

    private void UpdateFromRgb(int r, int g, int b)
    {
        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);
        HexValue = $"#{r:X2}{g:X2}{b:X2}";
    }

    public static void RgbToHsb(int r, int g, int b, out double h, out double s, out double br)
    {
        double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        br = max * 100.0;
        s = max == 0 ? 0 : (delta / max) * 100.0;

        if (delta == 0) { h = 0; }
        else if (max == rd) { h = 60.0 * (((gd - bd) / delta) % 6); }
        else if (max == gd) { h = 60.0 * (((bd - rd) / delta) + 2); }
        else { h = 60.0 * (((rd - gd) / delta) + 4); }

        if (h < 0) h += 360;
    }

    public static void HsbToRgb(double h, double s, double b, out int r, out int g, out int bl)
    {
        h = ((h % 360) + 360) % 360;
        double sf = Math.Clamp(s, 0, 100) / 100.0;
        double bf = Math.Clamp(b, 0, 100) / 100.0;

        double c = bf * sf;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = bf - c;

        double rd, gd, bd;
        if (h < 60)       { rd = c; gd = x; bd = 0; }
        else if (h < 120) { rd = x; gd = c; bd = 0; }
        else if (h < 180) { rd = 0; gd = c; bd = x; }
        else if (h < 240) { rd = 0; gd = x; bd = c; }
        else if (h < 300) { rd = x; gd = 0; bd = c; }
        else              { rd = c; gd = 0; bd = x; }

        r = (int)Math.Round((rd + m) * 255);
        g = (int)Math.Round((gd + m) * 255);
        bl = (int)Math.Round((bd + m) * 255);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ThemeDropdownItem
{
    public string Name { get; set; } = string.Empty;
    public bool IsPreset { get; set; }
    public override string ToString() => Name;
}

public class CustomThemeViewModel : INotifyPropertyChanged
{
    private readonly IConfigurationService _configService;
    private Configuration? _config;
    private string _startFromTheme = "Light";
    private ColorEntry? _selectedColor;
    private bool _isAdvancedMode;
    private bool _isDeriving;
    private bool _isWideLayout = true;
    private bool _showingColorPicker;
    private ThemeDropdownItem? _selectedDropdownTheme;

    private static readonly HashSet<string> BaseColorKeys = new()
    {
        "ThemeBackground", "ThemeText", "ThemeButtonBackground",
        "ThemeButtonText", "ThemeBorder", "ThemeHighlight"
    };

    private static readonly Dictionary<string, (string DisplayName, string Group)> ColorMetadata = new()
    {
        ["ThemeBackground"] = ("Background", "General"),
        ["ThemeText"] = ("Text", "General"),
        ["ThemePrimary"] = ("Primary", "General"),
        ["ThemeSecondary"] = ("Secondary", "General"),
        ["ThemeSurface"] = ("Surface", "General"),
        ["ThemeTextSecondary"] = ("Secondary Text", "General"),
        ["ThemeBorder"] = ("Border", "General"),
        ["ThemeButtonBackground"] = ("Button Background", "Buttons"),
        ["ThemeButtonText"] = ("Button Text", "Buttons"),
        ["ThemeButtonDisabledBg"] = ("Disabled Background", "Buttons"),
        ["ThemeButtonDisabledText"] = ("Disabled Text", "Buttons"),
        ["ThemeShellBackground"] = ("Shell Background", "Navigation"),
        ["ThemeShellText"] = ("Shell Text", "Navigation"),
        ["ThemeShellTabBar"] = ("Tab Bar", "Navigation"),
        ["ThemeShellTabSelected"] = ("Tab Selected", "Navigation"),
        ["ThemeShellTabUnselected"] = ("Tab Unselected", "Navigation"),
        ["ThemeSliderTrack"] = ("Slider Track", "Controls"),
        ["ThemeSliderTrackBg"] = ("Slider Background", "Controls"),
        ["ThemeEntryText"] = ("Entry Text", "Controls"),
        ["ThemeEntryPlaceholder"] = ("Entry Placeholder", "Controls"),
        ["ThemeCardBackground"] = ("Card Background", "Cards & Chips"),
        ["ThemeChipBackground"] = ("Chip Background", "Cards & Chips"),
        ["ThemeHighlight"] = ("Highlight", "Cards & Chips"),
        ["ThemeOverlay"] = ("Overlay", "Overlay"),
        ["ThemeOnOverlay"] = ("On Overlay", "Overlay"),
        ["ThemeError"] = ("Error", "Status"),
    };

    public PreviewColorDictionary PreviewColors { get; } = new();

    public CustomThemeViewModel(IConfigurationService configService)
    {
        _configService = configService;

        Colors = new ObservableCollection<ColorEntry>();
        FilteredColors = new ObservableCollection<ColorEntry>();
        StartFromThemes = new List<string> { "Light", "Dark", "Light Blue" };

        SaveCommand = new RelayCommand(async () => await SaveAsync());
        PreviewCommand = new RelayCommand(ApplyPreview);
        ResetCommand = new RelayCommand(() => ApplyThemeDefaults("Light"));
        StartFromCommand = new RelayCommand<string>(ApplyThemeDefaults);
        SelectionChangedCommand = new RelayCommand(OnSelectionChanged);
        ToggleModeCommand = new RelayCommand(ToggleMode);
        BackToListCommand = new RelayCommand(() => ShowingColorPicker = false);
        DeleteSavedThemeCommand = new RelayCommand(async () => await DeleteSelectedThemeAsync());
    }

    public ObservableCollection<ColorEntry> Colors { get; }
    public ObservableCollection<ColorEntry> FilteredColors { get; }
    public IReadOnlyList<string> StartFromThemes { get; }

    public ColorEntry? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor == value) return;
            // Clear previous selection
            if (_selectedColor != null)
                _selectedColor.IsSelected = false;
            _selectedColor = value;
            if (_selectedColor != null)
                _selectedColor.IsSelected = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedColor));
            OnPropertyChanged(nameof(SelectedColorKey));
            OnPropertyChanged(nameof(IsErrorSelected));
        }
    }

    public bool HasSelectedColor => _selectedColor != null;
    public string SelectedColorKey => _selectedColor?.Key ?? "";
    public bool IsErrorSelected => SelectedColorKey == "ThemeError";

    public string StartFromTheme
    {
        get => _startFromTheme;
        set
        {
            if (_startFromTheme == value) return;
            _startFromTheme = value;
            OnPropertyChanged();
            ApplyThemeDefaults(value);
        }
    }

    public bool IsAdvancedMode
    {
        get => _isAdvancedMode;
        set
        {
            if (_isAdvancedMode == value) return;
            _isAdvancedMode = value;
            OnPropertyChanged();
            if (!_isAdvancedMode)
                DeriveColorsFromBase();
            RefreshFilteredColors();
        }
    }

    public bool IsWideLayout
    {
        get => _isWideLayout;
        set
        {
            if (_isWideLayout == value) return;
            _isWideLayout = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNarrowLayout));
            OnPropertyChanged(nameof(ShowVariablesList));
            OnPropertyChanged(nameof(ShowColorPicker));
            OnPropertyChanged(nameof(ShowBackButton));
        }
    }

    public bool IsNarrowLayout => !_isWideLayout;

    public bool ShowingColorPicker
    {
        get => _showingColorPicker;
        set
        {
            if (_showingColorPicker == value) return;
            _showingColorPicker = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowVariablesList));
            OnPropertyChanged(nameof(ShowColorPicker));
            OnPropertyChanged(nameof(ShowBackButton));
        }
    }

    public bool ShowVariablesList => IsWideLayout || !ShowingColorPicker;
    public bool ShowColorPicker => IsWideLayout || ShowingColorPicker;
    public bool ShowBackButton => IsNarrowLayout && ShowingColorPicker;

    public ObservableCollection<ThemeDropdownItem> PresetAndSavedThemes { get; } = new();

    public ThemeDropdownItem? SelectedDropdownTheme
    {
        get => _selectedDropdownTheme;
        set
        {
            if (_selectedDropdownTheme == value) return;
            _selectedDropdownTheme = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanDeleteSelectedTheme));
            if (value != null)
                LoadDropdownTheme(value);
        }
    }

    public bool CanDeleteSelectedTheme => _selectedDropdownTheme is { IsPreset: false };

    public Func<string, Task<string?>>? PromptForName { get; set; }
    public Func<string, Task<bool>>? ConfirmDelete { get; set; }

    public ICommand SaveCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand StartFromCommand { get; }
    public ICommand SelectionChangedCommand { get; }
    public ICommand ToggleModeCommand { get; }
    public ICommand BackToListCommand { get; }
    public ICommand DeleteSavedThemeCommand { get; }

    public async Task LoadAsync()
    {
        _config = await _configService.LoadConfigurationAsync();

        Colors.Clear();
        var defaults = ThemeManager.LightDefaults;
        var saved = _config?.CustomThemeColors;

        foreach (var key in ThemeManager.AllColorKeys)
        {
            string displayName, group;
            if (ColorMetadata.TryGetValue(key, out var m))
            {
                displayName = m.DisplayName;
                group = m.Group;
            }
            else
            {
                displayName = key;
                group = "Other";
            }
            var hex = saved != null && saved.TryGetValue(key, out var sv) ? sv : defaults[key];
            var entry = new ColorEntry
            {
                Key = key,
                DisplayName = displayName,
                Group = group,
                HexValue = hex
            };
            // Auto-preview on any color change
            entry.PropertyChanged += OnColorEntryPropertyChanged;
            Colors.Add(entry);
            PreviewColors[key] = entry.ColorPreview;
        }

        RefreshFilteredColors();
        RefreshDropdownThemes();

        // Select first entry by default
        if (FilteredColors.Count > 0)
            SelectedColor = FilteredColors[0];
    }

    private void OnColorEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColorEntry.HexValue) && sender is ColorEntry entry)
        {
            PreviewColors[entry.Key] = entry.ColorPreview;
            if (!_isAdvancedMode && !_isDeriving && BaseColorKeys.Contains(entry.Key))
                DeriveColorsFromBase();
        }
    }

    private void RefreshFilteredColors()
    {
        FilteredColors.Clear();
        foreach (var entry in Colors)
        {
            if (_isAdvancedMode || BaseColorKeys.Contains(entry.Key))
                FilteredColors.Add(entry);
        }
        if (SelectedColor != null && !FilteredColors.Contains(SelectedColor) && FilteredColors.Count > 0)
            SelectedColor = FilteredColors[0];
    }

    private void ToggleMode()
    {
        IsAdvancedMode = !IsAdvancedMode;
    }

    private ColorEntry? FindEntry(string key) => Colors.FirstOrDefault(c => c.Key == key);

    private void DeriveColorsFromBase()
    {
        _isDeriving = true;
        try
        {
            var bg = FindEntry("ThemeBackground")?.ColorPreview ?? Microsoft.Maui.Graphics.Colors.White;
            var text = FindEntry("ThemeText")?.ColorPreview ?? Microsoft.Maui.Graphics.Colors.Black;
            var btnBg = FindEntry("ThemeButtonBackground")?.ColorPreview ?? Microsoft.Maui.Graphics.Colors.Blue;
            var btnText = FindEntry("ThemeButtonText")?.ColorPreview ?? Microsoft.Maui.Graphics.Colors.White;
            var border = FindEntry("ThemeBorder")?.ColorPreview ?? Microsoft.Maui.Graphics.Colors.Gray;
            var highlight = FindEntry("ThemeHighlight")?.ColorPreview ?? Microsoft.Maui.Graphics.Colors.Yellow;

            SetDerived("ThemeSurface", AdjustBrightness(bg, 0.10f));
            SetDerived("ThemeCardBackground", AdjustBrightness(bg, 0.10f));
            SetDerived("ThemeShellBackground", bg);
            SetDerived("ThemeShellTabBar", bg);
            SetDerived("ThemeOverlay", new Color(bg.Red, bg.Green, bg.Blue, 0.80f));
            SetDerived("ThemeSliderTrackBg", Lerp(bg, border, 0.20f));

            SetDerived("ThemeTextSecondary", new Color(text.Red, text.Green, text.Blue, 0.50f));
            SetDerived("ThemeShellText", text);
            SetDerived("ThemeEntryText", text);
            SetDerived("ThemeOnOverlay", text);
            SetDerived("ThemeShellTabSelected", text);

            SetDerived("ThemePrimary", btnBg);
            SetDerived("ThemeSliderTrack", btnBg);
            SetDerived("ThemeChipBackground", Lerp(btnBg, bg, 0.30f));

            SetDerived("ThemeShellTabUnselected", new Color(btnText.Red, btnText.Green, btnText.Blue, 0.60f));

            SetDerived("ThemeButtonDisabledBg", Lerp(border, bg, 0.40f));
            SetDerived("ThemeButtonDisabledText", border);
            SetDerived("ThemeEntryPlaceholder", border);

            SetDerived("ThemeSecondary", highlight);
            SetDerived("ThemeError", Lerp(highlight, new Color(1, 0, 0), 0.50f));
        }
        finally { _isDeriving = false; }
    }

    private void SetDerived(string key, Color color)
    {
        var entry = FindEntry(key);
        if (entry != null)
        {
            // For colors with alpha < 1, bake alpha into RGB against background for hex storage
            if (color.Alpha < 1.0f)
            {
                var bg = FindEntry("ThemeBackground")?.ColorPreview ?? Microsoft.Maui.Graphics.Colors.White;
                var r = color.Red * color.Alpha + bg.Red * (1 - color.Alpha);
                var g = color.Green * color.Alpha + bg.Green * (1 - color.Alpha);
                var b = color.Blue * color.Alpha + bg.Blue * (1 - color.Alpha);
                entry.HexValue = $"#{(int)(r * 255):X2}{(int)(g * 255):X2}{(int)(b * 255):X2}";
            }
            else
            {
                entry.HexValue = $"#{(int)(color.Red * 255):X2}{(int)(color.Green * 255):X2}{(int)(color.Blue * 255):X2}";
            }
        }
    }

    private static Color Lerp(Color a, Color b, float t)
    {
        return new Color(
            a.Red + (b.Red - a.Red) * t,
            a.Green + (b.Green - a.Green) * t,
            a.Blue + (b.Blue - a.Blue) * t);
    }

    private static Color AdjustBrightness(Color c, float amount)
    {
        return new Color(
            Math.Clamp(c.Red + amount, 0f, 1f),
            Math.Clamp(c.Green + amount, 0f, 1f),
            Math.Clamp(c.Blue + amount, 0f, 1f));
    }

    private void OnSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedColor));
        OnPropertyChanged(nameof(HasSelectedColor));
        OnPropertyChanged(nameof(SelectedColorKey));
        if (IsNarrowLayout && SelectedColor != null)
            ShowingColorPicker = true;
    }

    private void ApplyThemeDefaults(string? themeName)
    {
        if (string.IsNullOrEmpty(themeName)) return;
        var defaults = ThemeManager.GetThemeDefaults(themeName);
        foreach (var entry in Colors)
        {
            if (defaults.TryGetValue(entry.Key, out var hex))
                entry.HexValue = hex;
        }
    }

    public void ApplyPreview()
    {
        var dict = BuildColorDictionary();
        ThemeManager.ApplyCustomTheme(dict);
    }

    public async Task SaveAsync()
    {
        _config ??= await _configService.LoadConfigurationAsync();
        var dict = BuildColorDictionary();

        string? name = null;
        if (PromptForName != null)
            name = await PromptForName("Enter a unique name:");

        if (string.IsNullOrWhiteSpace(name))
            return;

        // Remove existing saved theme with same name
        _config.SavedCustomThemes.RemoveAll(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        _config.SavedCustomThemes.Add(new NamedCustomTheme { Name = name, Colors = new Dictionary<string, string>(dict) });

        // Also set as active custom theme
        _config.CustomThemeColors = dict;
        _config.Theme = "Custom";
        await _configService.SaveConfigurationAsync(_config);
        ThemeManager.ApplyCustomTheme(dict);
        RefreshDropdownThemes();
    }

    private async Task DeleteSelectedThemeAsync()
    {
        if (_selectedDropdownTheme == null || _selectedDropdownTheme.IsPreset) return;
        _config ??= await _configService.LoadConfigurationAsync();

        var name = _selectedDropdownTheme.Name;
        if (ConfirmDelete != null)
        {
            var confirmed = await ConfirmDelete($"Are you sure you want to permanently delete \"{name}\"?");
            if (!confirmed) return;
        }

        _config.SavedCustomThemes.RemoveAll(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        await _configService.SaveConfigurationAsync(_config);
        SelectedDropdownTheme = null;
        RefreshDropdownThemes();
    }

    private void LoadDropdownTheme(ThemeDropdownItem item)
    {
        if (item.IsPreset)
        {
            ApplyThemeDefaults(item.Name);
        }
        else
        {
            var saved = _config?.SavedCustomThemes.FirstOrDefault(t => t.Name == item.Name);
            if (saved != null)
            {
                foreach (var entry in Colors)
                {
                    if (saved.Colors.TryGetValue(entry.Key, out var hex))
                        entry.HexValue = hex;
                }
            }
        }
    }

    private void RefreshDropdownThemes()
    {
        PresetAndSavedThemes.Clear();
        foreach (var preset in StartFromThemes)
            PresetAndSavedThemes.Add(new ThemeDropdownItem { Name = preset, IsPreset = true });
        if (_config?.SavedCustomThemes != null)
        {
            foreach (var saved in _config.SavedCustomThemes)
                PresetAndSavedThemes.Add(new ThemeDropdownItem { Name = saved.Name, IsPreset = false });
        }
    }

    private Dictionary<string, string> BuildColorDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in Colors)
            dict[entry.Key] = entry.HexValue;
        return dict;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }

    private sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        public RelayCommand(Action<T?> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter is T t ? t : default);
    }
}
