using System.Text.Json;
using TabZeroAssistant.Core.Models;

namespace TabZeroAssistant.Core.Services;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(AppPaths.BaseDirectory);
        if (!File.Exists(AppPaths.SettingsPath))
        {
            var defaults = new AppSettings();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        await using var stream = File.OpenRead(AppPaths.SettingsPath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, SerializerOptions, cancellationToken);
        return settings ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(AppPaths.BaseDirectory);
        await using var stream = File.Create(AppPaths.SettingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
    }
}
