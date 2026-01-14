using Microsoft.Data.Sqlite;
using TabZeroAssistant.Core.Models;
using TabZeroAssistant.Core.Services;

namespace TabZeroAssistant.Core.Storage;

public sealed class SqliteStorage : IStorage
{
    private readonly string _connectionString;

    public SqliteStorage()
    {
        Directory.CreateDirectory(AppPaths.BaseDirectory);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = AppPaths.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var createMessages = """
            create table if not exists messages (
                id TEXT primary key,
                created_at TEXT not null,
                role TEXT not null,
                nonce BLOB not null,
                ciphertext BLOB not null
            );
            """;
        var createMemories = """
            create table if not exists memories (
                id TEXT primary key,
                created_at TEXT not null,
                type TEXT not null,
                score REAL not null,
                pinned INTEGER not null,
                tags TEXT not null,
                nonce BLOB not null,
                ciphertext BLOB not null
            );
            """;
        var createFeedback = """
            create table if not exists feedback (
                id TEXT primary key,
                created_at TEXT not null,
                target_type TEXT not null,
                target_id TEXT not null,
                rating INTEGER not null,
                nonce BLOB null,
                ciphertext BLOB null
            );
            """;
        var createEvents = """
            create table if not exists events (
                id TEXT primary key,
                created_at TEXT not null,
                app_name TEXT not null,
                window_title TEXT null,
                mode TEXT not null
            );
            """;

        await ExecuteNonQueryAsync(connection, createMessages, cancellationToken);
        await ExecuteNonQueryAsync(connection, createMemories, cancellationToken);
        await ExecuteNonQueryAsync(connection, createFeedback, cancellationToken);
        await ExecuteNonQueryAsync(connection, createEvents, cancellationToken);
    }

    public async Task SaveMessageAsync(MessageRecord record, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into messages (id, created_at, role, nonce, ciphertext)
            values ($id, $created_at, $role, $nonce, $ciphertext);
            """;
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", record.Id);
        command.Parameters.AddWithValue("$created_at", record.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$role", record.Role);
        command.Parameters.AddWithValue("$nonce", record.Nonce);
        command.Parameters.AddWithValue("$ciphertext", record.Ciphertext);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<MessageRecord>> LoadRecentMessagesAsync(int count, CancellationToken cancellationToken = default)
    {
        var results = new List<MessageRecord>();
        const string sql = """
            select id, created_at, role, nonce, ciphertext
            from messages
            order by created_at desc
            limit $count;
            """;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$count", count);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MessageRecord(
                reader.GetString(0),
                DateTimeOffset.Parse(reader.GetString(1)),
                reader.GetString(2),
                (byte[])reader[3],
                (byte[])reader[4]));
        }
        return results;
    }

    public async Task SaveMemoryAsync(MemoryRecord record, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into memories (id, created_at, type, score, pinned, tags, nonce, ciphertext)
            values ($id, $created_at, $type, $score, $pinned, $tags, $nonce, $ciphertext);
            """;
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", record.Id);
        command.Parameters.AddWithValue("$created_at", record.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$type", record.Type);
        command.Parameters.AddWithValue("$score", record.Score);
        command.Parameters.AddWithValue("$pinned", record.Pinned ? 1 : 0);
        command.Parameters.AddWithValue("$tags", record.Tags);
        command.Parameters.AddWithValue("$nonce", record.Nonce);
        command.Parameters.AddWithValue("$ciphertext", record.Ciphertext);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<MemoryRecord>> LoadRelevantMemoriesAsync(int count, CancellationToken cancellationToken = default)
    {
        var results = new List<MemoryRecord>();
        const string sql = """
            select id, created_at, type, score, pinned, tags, nonce, ciphertext
            from memories
            order by pinned desc, score desc, created_at desc
            limit $count;
            """;
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$count", count);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MemoryRecord(
                reader.GetString(0),
                DateTimeOffset.Parse(reader.GetString(1)),
                reader.GetString(2),
                reader.GetDouble(3),
                reader.GetInt32(4) == 1,
                reader.GetString(5),
                (byte[])reader[6],
                (byte[])reader[7]));
        }
        return results;
    }

    public async Task SaveFeedbackAsync(FeedbackRecord record, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into feedback (id, created_at, target_type, target_id, rating, nonce, ciphertext)
            values ($id, $created_at, $target_type, $target_id, $rating, $nonce, $ciphertext);
            """;
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", record.Id);
        command.Parameters.AddWithValue("$created_at", record.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$target_type", record.TargetType);
        command.Parameters.AddWithValue("$target_id", record.TargetId);
        command.Parameters.AddWithValue("$rating", record.Rating);
        command.Parameters.AddWithValue("$nonce", record.Nonce is null ? DBNull.Value : record.Nonce);
        command.Parameters.AddWithValue("$ciphertext", record.Ciphertext is null ? DBNull.Value : record.Ciphertext);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveEventAsync(EventRecord record, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into events (id, created_at, app_name, window_title, mode)
            values ($id, $created_at, $app_name, $window_title, $mode);
            """;
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", record.Id);
        command.Parameters.AddWithValue("$created_at", record.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$app_name", record.AppName);
        command.Parameters.AddWithValue("$window_title", record.WindowTitle is null ? DBNull.Value : record.WindowTitle);
        command.Parameters.AddWithValue("$mode", record.Mode);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<EventRecord>> LoadRecentEventsAsync(int count, CancellationToken cancellationToken = default)
    {
        var results = new List<EventRecord>();
        const string sql = """
            select id, created_at, app_name, window_title, mode
            from events
            order by created_at desc
            limit $count;
            """;
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$count", count);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new EventRecord(
                reader.GetString(0),
                DateTimeOffset.Parse(reader.GetString(1)),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetString(4)));
        }
        return results;
    }

    private static async Task ExecuteNonQueryAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
