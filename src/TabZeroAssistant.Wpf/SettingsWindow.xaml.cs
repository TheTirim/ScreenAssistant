using System.Windows;
using TabZeroAssistant.Core.Models;
using TabZeroAssistant.Core.Services;

namespace TabZeroAssistant.Wpf;

public partial class SettingsWindow : Window
{
    private readonly SettingsStore _settingsStore;
    private AppSettings _settings;
    private readonly Action<AppSettings> _onUpdated;

    public SettingsWindow(SettingsStore settingsStore, AppSettings settings, Action<AppSettings> onUpdated)
    {
        InitializeComponent();
        _settingsStore = settingsStore;
        _settings = settings;
        _onUpdated = onUpdated;
        LoadSettings();
        Hide();
    }

    private void LoadSettings()
    {
        TrackWindowTitles.IsChecked = _settings.TrackWindowTitles;
        ParanoiaMode.IsChecked = _settings.ParanoiaMode;
        foreach (ComboBoxItem item in ModeSelector.Items)
        {
            if (item.Tag is string tag && string.Equals(tag, _settings.Mode, StringComparison.OrdinalIgnoreCase))
            {
                ModeSelector.SelectedItem = item;
                break;
            }
        }
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        var selectedMode = (ModeSelector.SelectedItem as ComboBoxItem)?.Tag as string ?? "Work";
        _settings = _settings with
        {
            TrackWindowTitles = TrackWindowTitles.IsChecked == true,
            ParanoiaMode = ParanoiaMode.IsChecked == true,
            Mode = selectedMode
        };
        await _settingsStore.SaveAsync(_settings);
        _onUpdated(_settings);
        Hide();
    }
}
