using System.ComponentModel;
using System.Collections.ObjectModel;
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
    private readonly string _bubblePosition;
    private readonly int _maxRollingCharacters;
    private readonly int _maxSpecialKeyHistory;
    private readonly ObservableCollection<DisplayToken> _tokens = [];
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
        _maxRollingCharacters = settings.MaxTextLength;
        _maxSpecialKeyHistory = settings.MaxSpecialKeys;

        _hideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _hideTimer.Tick += HideTimerOnTick;
        TokenItemsControl.ItemsSource = _tokens;
        Loaded += (_, _) => PositionOverlay();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyOverlayWindowStyles();

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
            Topmost = true;
            PositionOverlay();
        }
        else
        {
            DisplayPanel.BeginAnimation(OpacityProperty, null);
            DisplayPanel.Opacity = 0;
        }
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
                    ProcessVisibleKey(e.VirtualKey);
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
        AddSpecialKey(
            KeyLabelFormatter.FormatChord(_pressedKeys, virtualKey),
            replaceModifierPreview: _hasModifierPreview);
        _hasModifierPreview = _maxSpecialKeyHistory > 0;
        RefreshDisplay();
    }

    private void ProcessVisibleKey(int virtualKey)
    {
        PrepareForInput();

        if (KeyLabelFormatter.HasShortcutModifier(_pressedKeys))
        {
            AddSpecialKey(
                KeyLabelFormatter.FormatChord(_pressedKeys, virtualKey),
                replaceModifierPreview: _hasModifierPreview);
            _hasModifierPreview = false;
        }
        else if (virtualKey == 0x08)
        {
            RemoveLastTextCharacter();
            AddSpecialKey("Backspace");
        }
        else if (virtualKey == 0x0D)
        {
            AddSpecialKey("Enter");
        }
        else if (virtualKey == 0x20)
        {
            AppendTextCharacter(' ');
            AddSpecialKey("Space");
        }
        else if (KeyLabelFormatter.TryGetPrintableCharacter(
                     virtualKey,
                     KeyLabelFormatter.IsShiftPressed(_pressedKeys),
                     IsCapsLockEnabled(),
                     out var character))
        {
            AppendTextCharacter(character);
            _hasModifierPreview = false;
        }
        else
        {
            AddSpecialKey(
                KeyLabelFormatter.FormatChord(_pressedKeys, virtualKey),
                replaceModifierPreview: _hasModifierPreview);
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

    private void AddSpecialKey(string label, bool replaceModifierPreview = false)
    {
        if (_maxSpecialKeyHistory == 0)
        {
            return;
        }

        if (replaceModifierPreview && _tokens.Count > 0 && _tokens[^1].IsSpecial)
        {
            _tokens.RemoveAt(_tokens.Count - 1);
        }

        _tokens.Add(new DisplayToken(label, IsSpecial: true));
        while (_tokens.Count(token => token.IsSpecial) > _maxSpecialKeyHistory)
        {
            var oldestSpecialIndex = FindFirstSpecialTokenIndex();
            if (oldestSpecialIndex < 0)
            {
                break;
            }

            _tokens.RemoveAt(oldestSpecialIndex);
            MergeAdjacentTextTokens();
        }
    }

    private void AppendTextCharacter(char character)
    {
        if (_maxRollingCharacters == 0)
        {
            return;
        }

        if (_tokens.Count > 0 && !_tokens[^1].IsSpecial)
        {
            var lastToken = _tokens[^1];
            _tokens[^1] = lastToken with { Value = lastToken.Value + character };
        }
        else
        {
            _tokens.Add(new DisplayToken(character.ToString(), IsSpecial: false));
        }

        TrimTextCharacters();
    }

    private void RemoveLastTextCharacter()
    {
        for (var index = _tokens.Count - 1; index >= 0; index--)
        {
            var token = _tokens[index];
            if (token.IsSpecial)
            {
                continue;
            }

            if (token.Value.Length == 1)
            {
                _tokens.RemoveAt(index);
            }
            else
            {
                _tokens[index] = token with { Value = token.Value[..^1] };
            }

            return;
        }
    }

    private void TrimTextCharacters()
    {
        var excess = _tokens.Where(token => !token.IsSpecial).Sum(token => token.Value.Length)
            - _maxRollingCharacters;

        for (var index = 0; index < _tokens.Count && excess > 0;)
        {
            var token = _tokens[index];
            if (token.IsSpecial)
            {
                index++;
                continue;
            }

            if (token.Value.Length <= excess)
            {
                excess -= token.Value.Length;
                _tokens.RemoveAt(index);
                continue;
            }

            _tokens[index] = token with { Value = token.Value[excess..] };
            excess = 0;
        }
    }

    private int FindFirstSpecialTokenIndex()
    {
        for (var index = 0; index < _tokens.Count; index++)
        {
            if (_tokens[index].IsSpecial)
            {
                return index;
            }
        }

        return -1;
    }

    private void MergeAdjacentTextTokens()
    {
        for (var index = 0; index < _tokens.Count - 1;)
        {
            if (!_tokens[index].IsSpecial && !_tokens[index + 1].IsSpecial)
            {
                _tokens[index] = _tokens[index] with
                {
                    Value = _tokens[index].Value + _tokens[index + 1].Value
                };
                _tokens.RemoveAt(index + 1);
                continue;
            }

            index++;
        }
    }

    private void RefreshDisplay()
    {
        if (_tokens.Count == 0)
        {
            DisplayPanel.BeginAnimation(OpacityProperty, null);
            DisplayPanel.Opacity = 0;
            return;
        }

        DisplayPanel.BeginAnimation(OpacityProperty, null);
        DisplayPanel.Opacity = 1;

        _hideTimer.Stop();
        _hideTimer.Start();
        Dispatcher.BeginInvoke(() => TokenScroller.ScrollToRightEnd(), DispatcherPriority.Loaded);
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
        _tokens.Clear();
        _hasModifierPreview = false;
    }

    private static bool IsCapsLockEnabled()
    {
        return (WindowsApi.GetKeyState(0x14) & 0x0001) != 0;
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
        Top = workArea.Bottom - ActualHeight - 48;
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
}
