using System.Diagnostics;
using TabZeroAssistant.Core.Models;

namespace TabZeroAssistant.Core.Services;

public sealed record ActionNotification(string Type, string? App, int? Minutes, string? Mode);

public sealed class ActionHandler
{
    private static readonly HashSet<string> AllowedApps = new(StringComparer.OrdinalIgnoreCase)
    {
        "notepad",
        "calc"
    };

    public Task HandleAsync(
        SuggestionAction action,
        Func<string, Task<bool>> confirmAsync,
        Func<string, Task> setModeAsync,
        Func<ActionNotification, Task> notifyAsync)
    {
        return action.Type switch
        {
            "start_timer" => StartTimerAsync(action, notifyAsync),
            "open_app" => OpenAppAsync(action, confirmAsync, notifyAsync),
            "set_mode" => SetModeAsync(action, setModeAsync, notifyAsync),
            _ => notifyAsync(new ActionNotification("unsupported", null, null, null))
        };
    }

    private static async Task StartTimerAsync(SuggestionAction action, Func<ActionNotification, Task> notifyAsync)
    {
        var minutes = action.Minutes ?? 25;
        await notifyAsync(new ActionNotification("timer_started", null, minutes, null));
    }

    private static async Task OpenAppAsync(
        SuggestionAction action,
        Func<string, Task<bool>> confirmAsync,
        Func<ActionNotification, Task> notifyAsync)
    {
        if (string.IsNullOrWhiteSpace(action.App))
        {
            await notifyAsync(new ActionNotification("missing_app", null, null, null));
            return;
        }

        var app = action.App.Trim();
        if (!AllowedApps.Contains(app))
        {
            var allowed = await confirmAsync(app);
            if (!allowed)
            {
                return;
            }
        }

        Process.Start(new ProcessStartInfo(app)
        {
            UseShellExecute = true
        });
        await notifyAsync(new ActionNotification("app_opened", app, null, null));
    }

    private static async Task SetModeAsync(
        SuggestionAction action,
        Func<string, Task> setModeAsync,
        Func<ActionNotification, Task> notifyAsync)
    {
        var mode = action.Mode ?? "Work";
        await setModeAsync(mode);
        await notifyAsync(new ActionNotification("mode_set", null, null, mode));
    }
}
