using System.Diagnostics;
using System.Net.Http.Json;

namespace TabZeroAssistant.Wpf;

public sealed class PythonServiceManager
{
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("http://127.0.0.1:8123")
    };

    private Process? _process;

    public async Task EnsureStartedAsync()
    {
        if (await IsHealthyAsync())
        {
            return;
        }

        var workingDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "python_service");
        if (!Directory.Exists(workingDir))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = "-m uvicorn app:app --host 127.0.0.1 --port 8123",
            WorkingDirectory = workingDir,
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        _process = Process.Start(startInfo);

        for (var i = 0; i < 10; i++)
        {
            if (await IsHealthyAsync())
            {
                break;
            }
            await Task.Delay(400);
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _client.GetFromJsonAsync<HealthResponse>("/health");
            return response?.Ok == true;
        }
        catch
        {
            return false;
        }
    }

    private sealed record HealthResponse(bool Ok);
}
