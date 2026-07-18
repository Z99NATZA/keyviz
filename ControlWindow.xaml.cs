using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using KeyViz.Native;
using KeyViz.Services;
using Button = System.Windows.Controls.Button;
using RepeatButton = System.Windows.Controls.Primitives.RepeatButton;

namespace KeyViz;

public partial class ControlWindow : Window
{
    private const double CollapsedWidth = 120;
    private const double CollapsedHeight = 50;
    private const double ExpandedWidth = 268;
    private const double ExpandedHeight = 254;

    private int _historyLength;
    private long _historyAdjustmentStartedAt;
    private bool _controlsVisible = true;
    private bool _isExpanded;
    private bool _overlayEnabled = true;

    internal ControlWindow(AppSettings settings)
    {
        _historyLength = settings.MaxHistoryLength;

        InitializeComponent();
        HistoryLengthText.Text = _historyLength.ToString();
        SetPositionState(settings.BubblePosition);
        Loaded += (_, _) => PositionControls();
    }

    internal event Action<bool>? OverlayVisibilityRequested;

    internal event Action<int>? MaxHistoryLengthRequested;

    internal event Action<string>? BubblePositionRequested;

    internal event Action<bool>? ControlsVisibilityRequested;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var handle = new WindowInteropHelper(this).Handle;
        var styles = WindowsApi.GetExtendedWindowStyle(handle);
        styles |= WindowsApi.WsExNoActivate;
        styles |= WindowsApi.WsExToolWindow;
        WindowsApi.SetExtendedWindowStyle(handle, styles);
    }

    internal void SetOverlayState(bool enabled)
    {
        _overlayEnabled = enabled;
        OverlayButton.Content = enabled ? "On" : "Off";
        SetButtonState(OverlayButton, enabled);
    }

    internal void SetControlsState(bool visible)
    {
        _controlsVisible = visible;
        ControlsButton.Content = visible ? "On" : "Off";
        SetButtonState(ControlsButton, visible);
    }

    private void KeyVizButtonOnClick(object sender, RoutedEventArgs e)
    {
        _isExpanded = !_isExpanded;

        if (_isExpanded)
        {
            Width = ExpandedWidth;
            Height = ExpandedHeight;
            PositionControls();
            PropertiesPanel.Visibility = Visibility.Visible;
        }
        else
        {
            PropertiesPanel.Visibility = Visibility.Collapsed;
            Width = CollapsedWidth;
            Height = CollapsedHeight;
            PositionControls();
        }
    }

    private void OverlayButtonOnClick(object sender, RoutedEventArgs e)
    {
        OverlayVisibilityRequested?.Invoke(!_overlayEnabled);
    }

    private void ControlsButtonOnClick(object sender, RoutedEventArgs e)
    {
        ControlsVisibilityRequested?.Invoke(!_controlsVisible);
    }

    private void HistoryButtonOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _historyAdjustmentStartedAt = Stopwatch.GetTimestamp();
    }

    private void HistoryButtonOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not RepeatButton { Tag: string directionText }
            || !int.TryParse(directionText, out var direction))
        {
            return;
        }

        var heldDuration = Stopwatch.GetElapsedTime(_historyAdjustmentStartedAt);
        var step = heldDuration.TotalSeconds switch
        {
            < 1 => 1,
            < 2.5 => 5,
            < 5 => 25,
            _ => 100
        };
        var nextValue = Math.Clamp(
            (long)_historyLength + (direction * step),
            1,
            int.MaxValue);

        SetHistoryLength((int)nextValue);
    }

    private void SetHistoryLength(int value)
    {
        var normalizedValue = Math.Max(1, value);
        if (normalizedValue == _historyLength)
        {
            return;
        }

        _historyLength = normalizedValue;
        HistoryLengthText.Text = _historyLength.ToString();
        MaxHistoryLengthRequested?.Invoke(_historyLength);
    }

    private void PositionButtonOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string position })
        {
            return;
        }

        SetPositionState(position);
        BubblePositionRequested?.Invoke(position);
    }

    private void SetPositionState(string position)
    {
        SetButtonState(LeftPositionButton, position == "left");
        SetButtonState(CenterPositionButton, position == "center");
        SetButtonState(RightPositionButton, position == "right");
    }

    private void PositionControls()
    {
        const double margin = 32;
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + margin;
        Top = workArea.Bottom - Height - margin;
    }

    private static System.Windows.Media.Brush GetThemeBrush(string resourceKey)
    {
        return (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource(resourceKey);
    }

    private static void SetButtonState(Button button, bool active)
    {
        button.Background = GetThemeBrush(active ? "Theme.ActionHoverBrush" : "Theme.InactiveControlBrush");
        button.BorderBrush = GetThemeBrush(active ? "Theme.ActionBorderBrush" : "Theme.InactiveBorderBrush");
        button.Foreground = GetThemeBrush(active ? "Theme.AppTextBrush" : "Theme.InactiveTextBrush");
    }
}
