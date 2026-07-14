using System.Windows;
using System.Windows.Interop;
using KeyViz.Native;

namespace KeyViz;

public partial class ControlWindow : Window
{
    public ControlWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => PositionControls();
    }

    internal event Action<bool>? OverlayVisibilityRequested;

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
        SetButtonState(ShowButton, enabled);
        SetButtonState(HideButton, !enabled);
    }

    private void ShowButtonOnClick(object sender, RoutedEventArgs e)
    {
        OverlayVisibilityRequested?.Invoke(true);
    }

    private void HideButtonOnClick(object sender, RoutedEventArgs e)
    {
        OverlayVisibilityRequested?.Invoke(false);
    }

    private void PositionControls()
    {
        const double margin = 32;
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + margin;
        Top = workArea.Bottom - ActualHeight - margin;
    }

    private static System.Windows.Media.Brush GetThemeBrush(string resourceKey)
    {
        return (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource(resourceKey);
    }

    private static void SetButtonState(System.Windows.Controls.Button button, bool active)
    {
        button.Background = GetThemeBrush(active ? "Theme.ActionHoverBrush" : "Theme.InactiveControlBrush");
        button.BorderBrush = GetThemeBrush(active ? "Theme.ActionBorderBrush" : "Theme.InactiveBorderBrush");
        button.Foreground = GetThemeBrush(active ? "Theme.AppTextBrush" : "Theme.InactiveTextBrush");
    }
}
