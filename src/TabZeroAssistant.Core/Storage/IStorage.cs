using TabZeroAssistant.Core.Models;

namespace TabZeroAssistant.Core.Storage;

public interface IStorage
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SaveMessageAsync(MessageRecord record, CancellationToken cancellationToken = default);
    Task<List<MessageRecord>> LoadRecentMessagesAsync(int count, CancellationToken cancellationToken = default);
    Task SaveMemoryAsync(MemoryRecord record, CancellationToken cancellationToken = default);
    Task<List<MemoryRecord>> LoadRelevantMemoriesAsync(int count, CancellationToken cancellationToken = default);
    Task SaveFeedbackAsync(FeedbackRecord record, CancellationToken cancellationToken = default);
    Task SaveEventAsync(EventRecord record, CancellationToken cancellationToken = default);
    Task<List<EventRecord>> LoadRecentEventsAsync(int count, CancellationToken cancellationToken = default);
}
