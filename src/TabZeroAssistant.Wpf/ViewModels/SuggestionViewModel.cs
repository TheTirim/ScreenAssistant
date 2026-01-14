using TabZeroAssistant.Core.Models;

namespace TabZeroAssistant.Wpf.ViewModels;

public sealed class SuggestionViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public List<SuggestionAction> Actions { get; init; } = [];
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
}
