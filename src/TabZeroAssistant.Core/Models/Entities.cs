using System.Text.Json.Serialization;

namespace TabZeroAssistant.Core.Models;

public sealed record MessageRecord(
    string Id,
    DateTimeOffset CreatedAt,
    string Role,
    byte[] Nonce,
    byte[] Ciphertext);

public sealed record MemoryRecord(
    string Id,
    DateTimeOffset CreatedAt,
    string Type,
    double Score,
    bool Pinned,
    string Tags,
    byte[] Nonce,
    byte[] Ciphertext);

public sealed record FeedbackRecord(
    string Id,
    DateTimeOffset CreatedAt,
    string TargetType,
    string TargetId,
    int Rating,
    byte[]? Nonce,
    byte[]? Ciphertext);

public sealed record EventRecord(
    string Id,
    DateTimeOffset CreatedAt,
    string AppName,
    string? WindowTitle,
    string Mode);

public sealed record ChatMessage(string Role, string Content);

public sealed record MemoryItem(string Type, string Content, double Score, bool Pinned, string Tags);

public sealed record MemoryCandidate(string Type, string Content, double Confidence);

public sealed record SuggestionAction
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("minutes")]
    public int? Minutes { get; init; }

    [JsonPropertyName("app")]
    public string? App { get; init; }

    [JsonPropertyName("mode")]
    public string? Mode { get; init; }
}

public sealed record Suggestion
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;

    [JsonPropertyName("actions")]
    public List<SuggestionAction> Actions { get; init; } = [];
}

public sealed record ChatResponse
{
    [JsonPropertyName("reply")]
    public string Reply { get; init; } = string.Empty;

    [JsonPropertyName("suggestions")]
    public List<Suggestion> Suggestions { get; init; } = [];

    [JsonPropertyName("memory_candidates")]
    public List<MemoryCandidate> MemoryCandidates { get; init; } = [];
}

public sealed record ChatRequest
{
    [JsonPropertyName("user_message")]
    public string UserMessage { get; init; } = string.Empty;

    [JsonPropertyName("mode")]
    public string Mode { get; init; } = string.Empty;

    [JsonPropertyName("recent_messages")]
    public List<ChatMessage> RecentMessages { get; init; } = [];

    [JsonPropertyName("memories")]
    public List<MemoryItem> Memories { get; init; } = [];

    [JsonPropertyName("events")]
    public List<EventRecord> Events { get; init; } = [];
}
