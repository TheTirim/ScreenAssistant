using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TabZeroAssistant.Core.Crypto;
using TabZeroAssistant.Core.Models;
using TabZeroAssistant.Core.Storage;

namespace TabZeroAssistant.Core.Services;

public sealed class ChatOrchestrator
{
    private readonly IStorage _storage;
    private readonly ICryptoService _cryptoService;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public ChatOrchestrator(IStorage storage, ICryptoService cryptoService, HttpClient httpClient)
    {
        _storage = storage;
        _cryptoService = cryptoService;
        _httpClient = httpClient;
    }

    public async Task<ChatResponse> SendAsync(string userMessage, AppSettings settings, CancellationToken cancellationToken = default)
    {
        await _storage.InitializeAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var userId = Guid.NewGuid().ToString("N");
        var userCipher = _cryptoService.Encrypt(Encoding.UTF8.GetBytes(userMessage), BuildAad("message", userId));
        await _storage.SaveMessageAsync(new MessageRecord(
            userId,
            now,
            "user",
            userCipher.Nonce,
            userCipher.Ciphertext),
            cancellationToken);

        await RecordSelfEventAsync(settings, cancellationToken);

        var recentMessages = await LoadRecentMessagesAsync(8, cancellationToken);
        var memories = await LoadRelevantMemoriesAsync(6, cancellationToken);
        var events = await _storage.LoadRecentEventsAsync(10, cancellationToken);

        var request = new ChatRequest
        {
            UserMessage = userMessage,
            Mode = settings.Mode,
            RecentMessages = recentMessages,
            Memories = memories,
            Events = events
        };

        ChatResponse? response = null;
        try
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync("/chat", request, cancellationToken);
            response = await httpResponse.Content.ReadFromJsonAsync<ChatResponse>(_serializerOptions, cancellationToken);
        }
        catch
        {
            // No plaintext logging. Fallback below.
        }

        response ??= BuildFallbackResponse(settings);

        var assistantId = Guid.NewGuid().ToString("N");
        var assistantCipher = _cryptoService.Encrypt(Encoding.UTF8.GetBytes(response.Reply), BuildAad("message", assistantId));
        await _storage.SaveMessageAsync(new MessageRecord(
            assistantId,
            DateTimeOffset.UtcNow,
            "assistant",
            assistantCipher.Nonce,
            assistantCipher.Ciphertext),
            cancellationToken);

        await PersistHighConfidenceMemoriesAsync(response.MemoryCandidates, cancellationToken);
        return response;
    }

    public async Task SaveFeedbackAsync(string targetType, string targetId, int rating, CancellationToken cancellationToken = default)
    {
        var record = new FeedbackRecord(
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            targetType,
            targetId,
            rating,
            null,
            null);
        await _storage.SaveFeedbackAsync(record, cancellationToken);
    }

    public async Task SaveMemoryCandidateAsync(MemoryCandidate candidate, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N");
        var payload = Encoding.UTF8.GetBytes(candidate.Content);
        var cipher = _cryptoService.Encrypt(payload, BuildAad("memory", id));
        var record = new MemoryRecord(
            id,
            DateTimeOffset.UtcNow,
            candidate.Type,
            candidate.Confidence,
            false,
            string.Empty,
            cipher.Nonce,
            cipher.Ciphertext);
        await _storage.SaveMemoryAsync(record, cancellationToken);
    }

    private async Task<List<ChatMessage>> LoadRecentMessagesAsync(int count, CancellationToken cancellationToken)
    {
        var records = await _storage.LoadRecentMessagesAsync(count, cancellationToken);
        var messages = new List<ChatMessage>();
        foreach (var record in records.OrderBy(r => r.CreatedAt))
        {
            var plaintext = _cryptoService.Decrypt(record.Nonce, record.Ciphertext, BuildAad("message", record.Id));
            messages.Add(new ChatMessage(record.Role, Encoding.UTF8.GetString(plaintext)));
        }
        return messages;
    }

    private async Task<List<MemoryItem>> LoadRelevantMemoriesAsync(int count, CancellationToken cancellationToken)
    {
        var records = await _storage.LoadRelevantMemoriesAsync(count, cancellationToken);
        var memories = new List<MemoryItem>();
        foreach (var record in records)
        {
            var plaintext = _cryptoService.Decrypt(record.Nonce, record.Ciphertext, BuildAad("memory", record.Id));
            memories.Add(new MemoryItem(record.Type, Encoding.UTF8.GetString(plaintext), record.Score, record.Pinned, record.Tags));
        }
        return memories;
    }

    private async Task PersistHighConfidenceMemoriesAsync(IEnumerable<MemoryCandidate> candidates, CancellationToken cancellationToken)
    {
        foreach (var candidate in candidates.Where(c => c.Confidence >= 0.85))
        {
            await SaveMemoryCandidateAsync(candidate, cancellationToken);
        }
    }

    private async Task RecordSelfEventAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        var record = new EventRecord(
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            "TabZeroAssistant",
            settings.TrackWindowTitles ? "Chat" : null,
            settings.Mode);
        await _storage.SaveEventAsync(record, cancellationToken);
    }

    private static byte[] BuildAad(string type, string id)
    {
        return Encoding.UTF8.GetBytes($"{type}:{id}");
    }

    private static ChatResponse BuildFallbackResponse(AppSettings settings)
    {
        var isGerman = System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
        var reply = settings.Mode switch
        {
            "Study" => isGerman
                ? "Ich bin gerade offline. Worauf möchtest du dich als Nächstes konzentrieren?"
                : "I can keep this offline. Tell me what you want to focus on next.",
            "Evening" => isGerman
                ? "Ich bin gerade offline. Soll ich dir einen kurzen Abend-Plan vorschlagen?"
                : "Offline for now. Want a quick wind-down plan?",
            _ => isGerman
                ? "Ich bin gerade offline, kann aber Notizen und Vorschläge festhalten."
                : "I'm offline right now, but I can still keep notes and suggestions."
        };
        return new ChatResponse
        {
            Reply = reply,
            Suggestions = []
        };
    }
}
