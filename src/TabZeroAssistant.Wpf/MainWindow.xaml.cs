using System.Windows;
using TabZeroAssistant.Core.Models;
using TabZeroAssistant.Core.Services;
using TabZeroAssistant.Wpf.ViewModels;

namespace TabZeroAssistant.Wpf;

public partial class MainWindow : Window
{
    private readonly ChatOrchestrator _orchestrator;
    private readonly SettingsStore _settingsStore;
    private readonly PythonServiceManager _pythonServiceManager;
    private readonly ActionHandler _actionHandler = new();
    private AppSettings _settings;
    private readonly MainWindowViewModel _viewModel = new();

    public MainWindow(ChatOrchestrator orchestrator, SettingsStore settingsStore, PythonServiceManager pythonServiceManager, AppSettings settings)
    {
        InitializeComponent();
        _orchestrator = orchestrator;
        _settingsStore = settingsStore;
        _pythonServiceManager = pythonServiceManager;
        _settings = settings;
        DataContext = _viewModel;
        Hide();
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
    }

    private async void OnSendClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.InputText))
        {
            return;
        }

        var input = _viewModel.InputText.Trim();
        _viewModel.InputText = string.Empty;
        _viewModel.ChatMessages.Add(new ChatMessageViewModel { Role = "user", Content = input });

        if (!await _pythonServiceManager.IsHealthyAsync())
        {
            _viewModel.StatusMessage = Resources.Strings.NoServiceWarning;
        }
        else
        {
            _viewModel.StatusMessage = string.Empty;
        }

        ChatResponse response;
        try
        {
            response = await _orchestrator.SendAsync(input, _settings);
        }
        catch
        {
            response = new ChatResponse { Reply = Resources.Strings.NoServiceWarning };
        }

        _viewModel.ChatMessages.Add(new ChatMessageViewModel { Role = "assistant", Content = response.Reply });

        _viewModel.Suggestions.Clear();
        foreach (var suggestion in response.Suggestions)
        {
            _viewModel.Suggestions.Add(new SuggestionViewModel
            {
                Title = suggestion.Title,
                Reason = suggestion.Reason,
                Actions = suggestion.Actions
            });
        }

        _viewModel.MemoryCandidates.Clear();
        foreach (var candidate in response.MemoryCandidates.Where(c => c.Confidence < 0.85))
        {
            _viewModel.MemoryCandidates.Add(new MemoryCandidateViewModel
            {
                Candidate = candidate,
                Selected = true
            });
        }
        _viewModel.MemoryCandidatesVisible = _viewModel.MemoryCandidates.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnFeedbackPositive(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is SuggestionViewModel suggestion)
        {
            await _orchestrator.SaveFeedbackAsync("suggestion", suggestion.Id, 1);
        }
    }

    private async void OnFeedbackNegative(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is SuggestionViewModel suggestion)
        {
            await _orchestrator.SaveFeedbackAsync("suggestion", suggestion.Id, -1);
        }
    }

    private async void OnFeedbackNever(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is SuggestionViewModel suggestion)
        {
            await _orchestrator.SaveFeedbackAsync("suggestion", suggestion.Id, -2);
        }
    }

    private async void OnSaveMemories(object sender, RoutedEventArgs e)
    {
        var selected = _viewModel.MemoryCandidates.Where(c => c.Selected).ToList();
        foreach (var candidate in selected)
        {
            await _orchestrator.SaveMemoryCandidateAsync(candidate.Candidate);
        }
        _viewModel.MemoryCandidates.Clear();
        _viewModel.MemoryCandidatesVisible = Visibility.Collapsed;
    }

    private async void OnActionClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is SuggestionAction action)
        {
            await _actionHandler.HandleAsync(action, ConfirmActionAsync, SetModeAsync, NotifyAsync);
        }
    }

    private Task<bool> ConfirmActionAsync(string app)
    {
        var message = string.Format(Resources.Strings.ConfirmOpenApp, app);
        var result = MessageBox.Show(message, Resources.Strings.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    private async Task SetModeAsync(string mode)
    {
        _settings = _settings with { Mode = mode };
        await _settingsStore.SaveAsync(_settings);
    }

    private Task NotifyAsync(ActionNotification notification)
    {
        _viewModel.StatusMessage = notification.Type switch
        {
            "timer_started" => string.Format(Resources.Strings.NotificationTimerStarted, notification.Minutes ?? 25),
            "app_opened" => string.Format(Resources.Strings.NotificationAppOpened, notification.App),
            "mode_set" => string.Format(Resources.Strings.NotificationModeSet, notification.Mode),
            "missing_app" => Resources.Strings.NotificationMissingApp,
            "unsupported" => Resources.Strings.NotificationUnsupported,
            _ => Resources.Strings.NoServiceWarning
        };
        return Task.CompletedTask;
    }
}
