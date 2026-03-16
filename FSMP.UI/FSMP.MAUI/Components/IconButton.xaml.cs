using System.Windows.Input;

namespace FSMP.MAUI.Components;

public partial class IconButton : ContentView
{
    /// <summary>
    /// Window width below which the label text is hidden, showing icon only.
    /// </summary>
    private const double CompactThreshold = 600;

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(IconButton), string.Empty, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(IconButton), string.Empty, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(IconButton));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(IconButton));

    public static readonly BindableProperty ButtonWidthRequestProperty =
        BindableProperty.Create(nameof(ButtonWidthRequest), typeof(double), typeof(IconButton), -1.0, propertyChanged: OnButtonWidthChanged);

    public static readonly BindableProperty ButtonTextColorProperty =
        BindableProperty.Create(nameof(ButtonTextColor), typeof(Color), typeof(IconButton), null, propertyChanged: OnTextColorChanged);

    private bool _isCompact;
    private Window? _trackedWindow;

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public double ButtonWidthRequest
    {
        get => (double)GetValue(ButtonWidthRequestProperty);
        set => SetValue(ButtonWidthRequestProperty, value);
    }

    public Color ButtonTextColor
    {
        get => (Color)GetValue(ButtonTextColorProperty);
        set => SetValue(ButtonTextColorProperty, value);
    }

    public IconButton()
    {
        InitializeComponent();
        UpdateText();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        SubscribeToWindow();
    }

    private void SubscribeToWindow()
    {
        // Unsubscribe from previous window if any
        if (_trackedWindow is not null)
        {
            _trackedWindow.SizeChanged -= OnWindowSizeChanged;
            _trackedWindow = null;
        }

        var window = this.Window;
        if (window is null)
            return;

        _trackedWindow = window;
        _trackedWindow.SizeChanged += OnWindowSizeChanged;

        // Evaluate immediately with current size
        EvaluateCompact(window.Width);
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        if (_trackedWindow is not null)
            EvaluateCompact(_trackedWindow.Width);
    }

    private void EvaluateCompact(double windowWidth)
    {
        var compact = windowWidth > 0 && windowWidth < CompactThreshold;
        if (compact != _isCompact)
        {
            _isCompact = compact;
            UpdateText();
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is IconButton btn)
            btn.UpdateText();
    }

    private static void OnButtonWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is IconButton btn && newValue is double width)
            btn.InnerButton.WidthRequest = width;
    }

    private static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is IconButton btn && newValue is Color color)
            btn.InnerButton.TextColor = color;
    }

    private void UpdateText()
    {
        if (_isCompact || string.IsNullOrEmpty(Label))
            InnerButton.Text = Icon;
        else
            InnerButton.Text = $"{Icon} {Label}";
    }

    private void OnClicked(object? sender, EventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }
}
