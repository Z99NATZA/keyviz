using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using KeyViz.Models;
using KeyViz.Native;
using KeyViz.Services;

namespace KeyViz;

public partial class MainWindow : Window
{
    private readonly HashSet<int> _pressedKeys = [];
    private string _bubblePosition;
    private readonly DisplayHistory _history;
    private readonly DispatcherTimer _hideTimer;
    private HwndSource? _windowSource;
    private RawKeyboardInput? _keyboardInput;
    private bool _overlayEnabled = true;
    private bool _hasModifierPreview;
    private bool _startFreshGroup;

    internal bool IsOverlayEnabled => _overlayEnabled;

    internal MainWindow(AppSettings settings)
    {
        InitializeComponent();

        _bubblePosition = settings.BubblePosition;
        _history = new DisplayHistory(settings.MaxHistoryLength);
        SetOverlayWidthLimit();

        _hideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _hideTimer.Tick += HideTimerOnTick;
        TokenItemsControl.ItemsSource = _history.Tokens;
        Loaded += (_, _) => PositionOverlay();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyOverlayWindowStyles();
        ReassertTopmost();

        try
        {
            var handle = new WindowInteropHelper(this).Handle;
            _windowSource = HwndSource.FromHwnd(handle)
                ?? throw new InvalidOperationException("The overlay window source is unavailable.");
            _windowSource.AddHook(WindowMessageHook);
            _keyboardInput = new RawKeyboardInput(handle);
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            System.Windows.MessageBox.Show(
                $"KeyViz could not start the keyboard listener.\n\n{exception.Message}",
                "KeyViz",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown(1);
        }
    }

    internal void SetOverlayEnabled(bool enabled)
    {
        _overlayEnabled = enabled;
        _hideTimer.Stop();
        ClearDisplay();

        if (enabled)
        {
            ReassertTopmost();
            PositionOverlay();
        }
        else
        {
            DisplayPanel.BeginAnimation(OpacityProperty, null);
            DisplayPanel.Opacity = 0;
        }
    }

    internal void SetMaxHistoryLength(int maxLength)
    {
        _history.SetMaxLength(maxLength);
        if (!_history.CanStoreTokens)
        {
            _hasModifierPreview = false;
        }

        RefreshDisplay();
    }

    internal void SetBubblePosition(string position)
    {
        _bubblePosition = position;
        PositionOverlay();
    }

    protected override void OnClosed(EventArgs e)
    {
        _keyboardInput?.Dispose();
        _windowSource?.RemoveHook(WindowMessageHook);
        base.OnClosed(e);
    }

    private IntPtr WindowMessageHook(
        IntPtr windowHandle,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (message == WindowsApi.WmInput
            && _keyboardInput?.TryRead(lParam, out var keyboardEvent) == true)
        {
            HandleKeyEvent(keyboardEvent);
        }

        return IntPtr.Zero;
    }

    private void HandleKeyEvent(KeyboardInputEvent e)
    {
        if (e.IsKeyDown)
        {
            var firstPress = _pressedKeys.Add(e.VirtualKey);

            if (_overlayEnabled)
            {
                if (KeyLabelFormatter.IsModifier(e.VirtualKey))
                {
                    if (firstPress)
                    {
                        ProcessModifierKey(e.VirtualKey);
                    }
                }
                else
                {
                    ProcessVisibleKey(e);
                }
            }
        }
        else
        {
            _pressedKeys.Remove(e.VirtualKey);
            if (KeyLabelFormatter.IsModifier(e.VirtualKey))
            {
                _hasModifierPreview = false;
            }
        }
    }

    private void ProcessModifierKey(int virtualKey)
    {
        PrepareForInput();
        _history.AddSpecial(
            KeyLabelFormatter.FormatChord(_pressedKeys, virtualKey),
            replaceLastSpecial: _hasModifierPreview);
        _hasModifierPreview = _history.CanStoreTokens;
        RefreshDisplay();
    }

    private void ProcessVisibleKey(KeyboardInputEvent keyboardEvent)
    {
        PrepareForInput();
        var virtualKey = keyboardEvent.VirtualKey;

        if (KeyLabelFormatter.HasShortcutModifier(_pressedKeys))
        {
            _history.AddSpecial(
                KeyLabelFormatter.FormatChord(_pressedKeys, virtualKey),
                replaceLastSpecial: _hasModifierPreview);
            _hasModifierPreview = false;
        }
        else if (virtualKey == 0x08)
        {
            _history.RemoveLastTextCodePoint();
            _history.AddSpecial("Backspace");
        }
        else if (virtualKey == 0x0D)
        {
            _history.AddSpecial("Enter");
        }
        else if (virtualKey == 0x20)
        {
            _history.AppendText(" ");
        }
        else if (KeyboardTextTranslator.TryTranslate(
                     keyboardEvent,
                     _pressedKeys,
                     out var text))
        {
            _history.AppendText(text);
            _hasModifierPreview = false;
        }
        else
        {
            _history.AddSpecial(
                KeyLabelFormatter.FormatChord(_pressedKeys, virtualKey),
                replaceLastSpecial: _hasModifierPreview);
        }

        _hasModifierPreview = false;
        RefreshDisplay();
    }

    private void PrepareForInput()
    {
        if (!_startFreshGroup)
        {
            return;
        }

        ClearDisplay();
        _startFreshGroup = false;
    }

    private void RefreshDisplay()
    {
        if (_history.Tokens.Count == 0)
        {
            _hideTimer.Stop();
            DisplayPanel.BeginAnimation(OpacityProperty, null);
            DisplayPanel.Opacity = 0;
            return;
        }

        ReassertTopmost();
        DisplayPanel.BeginAnimation(OpacityProperty, null);
        DisplayPanel.Opacity = 1;

        _hideTimer.Stop();
        _hideTimer.Start();
        Dispatcher.BeginInvoke(
            () =>
            {
                PositionOverlay();
                TokenScroller.ScrollToRightEnd();
            },
            DispatcherPriority.Loaded);
    }

    private void HideTimerOnTick(object? sender, EventArgs e)
    {
        _hideTimer.Stop();
        _startFreshGroup = true;
        DisplayPanel.BeginAnimation(
            OpacityProperty,
            new DoubleAnimation(0, TimeSpan.FromMilliseconds(260))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
    }

    private void ClearDisplay()
    {
        _history.Clear();
        _hasModifierPreview = false;
    }

    private void PositionOverlay()
    {
        const double edgeMargin = 32;
        var workArea = SystemParameters.WorkArea;
        var horizontalFraction = _bubblePosition switch
        {
            "left" => 0.25,
            "right" => 0.75,
            _ => 0.5
        };
        var desiredLeft = workArea.Left + (workArea.Width * horizontalFraction) - (ActualWidth / 2);
        var minimumLeft = workArea.Left + edgeMargin;
        var maximumLeft = Math.Max(minimumLeft, workArea.Right - ActualWidth - edgeMargin);

        Left = Math.Clamp(desiredLeft, minimumLeft, maximumLeft);
        Top = workArea.Bottom - ActualHeight - edgeMargin;
    }

    private void SetOverlayWidthLimit()
    {
        const double edgeMargin = 32;
        var availableWidth = Math.Max(1, SystemParameters.WorkArea.Width - (edgeMargin * 2));
        MaxWidth = availableWidth;
        DisplayPanel.MaxWidth = availableWidth;
    }

    private void ApplyOverlayWindowStyles()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var styles = WindowsApi.GetExtendedWindowStyle(handle);
        styles |= WindowsApi.WsExTransparent;
        styles |= WindowsApi.WsExNoActivate;
        styles |= WindowsApi.WsExToolWindow;
        WindowsApi.SetExtendedWindowStyle(handle, styles);
    }

    private void ReassertTopmost()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        _ = WindowsApi.SetWindowPos(
            handle,
            WindowsApi.HwndTopmost,
            0,
            0,
            0,
            0,
            WindowsApi.SwpNoMove
            | WindowsApi.SwpNoSize
            | WindowsApi.SwpNoActivate
            | WindowsApi.SwpNoOwnerZOrder);
    }
}
