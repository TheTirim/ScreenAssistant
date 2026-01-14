using TabZeroAssistant.Core.Models;

namespace TabZeroAssistant.Wpf.ViewModels;

public sealed class MemoryCandidateViewModel
{
    public MemoryCandidate Candidate { get; init; } = new("preference", string.Empty, 0);
    public bool Selected { get; set; }
}
