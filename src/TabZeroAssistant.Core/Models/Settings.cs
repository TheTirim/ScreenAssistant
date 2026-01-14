namespace TabZeroAssistant.Core.Models;

public sealed record AppSettings
{
    public bool TrackWindowTitles { get; init; }
    public bool ParanoiaMode { get; init; }
    public string Mode { get; init; } = "Work";
}
