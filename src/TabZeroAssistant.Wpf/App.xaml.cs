using System.Windows;
using System.Windows.Forms;
using TabZeroAssistant.Core.Crypto;
using TabZeroAssistant.Core.Services;
using TabZeroAssistant.Core.Storage;

namespace TabZeroAssistant.Wpf;

public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private SettingsWindow? _settingsWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var storage = new SqliteStorage();
        var cryptoService = new AesGcmCryptoService(new DpapiKeyStore());
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8123")
        };
        var orchestrator = new ChatOrchestrator(storage, cryptoService, httpClient);
        var settingsStore = new SettingsStore();
        var settings = await settingsStore.LoadAsync();

        var pythonManager = new PythonServiceManager();
        await pythonManager.EnsureStartedAsync();

        _mainWindow = new MainWindow(orchestrator, settingsStore, pythonManager, settings);
        _settingsWindow = new SettingsWindow(settingsStore, settings, OnSettingsUpdated);

        InitializeTray();
    }

    private void InitializeTray()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = Resources.Strings.AppTitle,
            Visible = true,
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip()
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();

        _notifyIcon.ContextMenuStrip.Items.Add(Resources.Strings.MenuOpen, null, (_, _) => ShowMainWindow());
        _notifyIcon.ContextMenuStrip.Items.Add(Resources.Strings.MenuSettings, null, (_, _) => ShowSettingsWindow());
        _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _notifyIcon.ContextMenuStrip.Items.Add(Resources.Strings.MenuExit, null, (_, _) => ExitApplication());
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void ShowSettingsWindow()
    {
        if (_settingsWindow is null)
        {
            return;
        }

        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private void ExitApplication()
    {
        _notifyIcon?.Dispose();
        _mainWindow?.Close();
        _settingsWindow?.Close();
        Shutdown();
    }

    private void OnSettingsUpdated(TabZeroAssistant.Core.Models.AppSettings settings)
    {
        _mainWindow?.ApplySettings(settings);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}
