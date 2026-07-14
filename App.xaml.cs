using System.Drawing;
using System.Windows.Forms;
using KeyViz.Native;
using KeyViz.Services;

namespace KeyViz;

public partial class App : System.Windows.Application
{
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

        var settings = SettingsService.Load();

        _overlay = new MainWindow(settings);
        MainWindow = _overlay;
        _overlay.Show();

        if (Dispatcher.HasShutdownStarted)
        {
            return;
        }

        if (settings.ShowControls)
        {
            _controls = new ControlWindow();
            _controls.OverlayVisibilityRequested += SetOverlayEnabled;
            _controls.SetOverlayState(true);
            _controls.Show();
        }

        var menu = new ContextMenuStrip();
        menu.Items.Add("Show", null, (_, _) =>
            Dispatcher.Invoke(() => SetOverlayEnabled(true)));
        menu.Items.Add("Hide", null, (_, _) =>
            Dispatcher.Invoke(() => SetOverlayEnabled(false)));
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

    private void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        Dispatcher.BeginInvoke(() => Shutdown());
    }
}
