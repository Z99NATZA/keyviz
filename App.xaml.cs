using System.Drawing;
using System.Windows.Forms;
using KeyViz.Native;
using KeyViz.Services;

namespace KeyViz;

public partial class App : System.Windows.Application
{
    private AppSettings _settings = new();
    private NotifyIcon? _trayIcon;
    private ControlWindow? _controls;
    private MainWindow? _overlay;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        if (WindowsApi.GetConsoleWindow() != IntPtr.Zero)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
        }

        _settings = SettingsService.Load();

        _overlay = new MainWindow(_settings);
        MainWindow = _overlay;
        _overlay.Show();

        if (Dispatcher.HasShutdownStarted)
        {
            return;
        }

        if (_settings.ShowControls)
        {
            ShowControls();
        }

        var menu = new ContextMenuStrip();
        menu.Items.Add("Show keystrokes", null, (_, _) =>
            Dispatcher.Invoke(() => SetOverlayEnabled(true)));
        menu.Items.Add("Hide keystrokes", null, (_, _) =>
            Dispatcher.Invoke(() => SetOverlayEnabled(false)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Show Keyviz button", null, (_, _) =>
            Dispatcher.Invoke(() => SetControlsVisible(true)));
        menu.Items.Add("Hide Keyviz button", null, (_, _) =>
            Dispatcher.Invoke(() => SetControlsVisible(false)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => Dispatcher.Invoke(() => Shutdown()));

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "KeyViz",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) =>
            Dispatcher.Invoke(() => SetOverlayEnabled(!_overlay.IsOverlayEnabled));
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        Console.CancelKeyPress -= ConsoleOnCancelKeyPress;

        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.ContextMenuStrip?.Dispose();
            _trayIcon.Dispose();
        }

        base.OnExit(e);
    }

    private void SetOverlayEnabled(bool enabled)
    {
        _overlay?.SetOverlayEnabled(enabled);
        _controls?.SetOverlayState(enabled);
    }

    private void SetMaxHistoryLength(int maxLength)
    {
        _settings = _settings with { MaxHistoryLength = maxLength };
        _overlay?.SetMaxHistoryLength(_settings.MaxHistoryLength);
        SettingsService.Save(_settings);
    }

    private void SetControlsVisible(bool visible)
    {
        _settings = _settings with { ShowControls = visible };

        if (visible)
        {
            ShowControls();
        }
        else
        {
            _controls?.SetControlsState(false);
            _controls?.Hide();
        }

        SettingsService.Save(_settings);
    }

    private void ShowControls()
    {
        if (_controls is null)
        {
            _controls = new ControlWindow(_settings);
            _controls.OverlayVisibilityRequested += SetOverlayEnabled;
            _controls.MaxHistoryLengthRequested += SetMaxHistoryLength;
            _controls.BubblePositionRequested += SetBubblePosition;
            _controls.ControlsVisibilityRequested += SetControlsVisible;
        }

        _controls.SetOverlayState(_overlay?.IsOverlayEnabled ?? false);
        _controls.SetControlsState(true);
        _controls.Show();
    }

    private void SetBubblePosition(string position)
    {
        _settings = (_settings with { BubblePosition = position }).Normalize();
        _overlay?.SetBubblePosition(_settings.BubblePosition);
        SettingsService.Save(_settings);
    }

    private void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        Dispatcher.BeginInvoke(() => Shutdown());
    }
}
