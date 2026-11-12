using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace laba;

public class SemesterRepository(NpgsqlDataSource dataSource)
{
    public async Task InitializeAsync(
        CancellationToken ct = default)
    {
        const string sql = """
                           CREATE TABLE IF NOT EXISTS semester_snapshots
                           (
                               session_id uuid NOT NULL,
                               day integer NOT NULL CHECK (day >= 0),
                               state jsonb NOT NULL,
                               saved_at timestamptz NOT NULL DEFAULT now(),
                               
                               PRIMARY KEY (session_id, day)
                           );
                           """;
        await using var command = dataSource.CreateCommand(sql);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task SaveAsync(
        Guid sessionId,
        SemesterState state,
        CancellationToken ct = default)
    {
        const string sql = """
                           INSERT INTO semester_snapshots (session_id, day, state)
                           VALUES ($1, $2, $3);
                           """;
        await using var command = dataSource.CreateCommand(sql);
        
        command.Parameters.AddWithValue(sessionId);
        command.Parameters.AddWithValue(state.Day);
        
        command.Parameters.AddWithValue(
            NpgsqlDbType.Jsonb,
            JsonSerializer.Serialize(state));
        
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<SemesterState> LoadAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        const string sql = """
                           SELECT state::text
                           FROM semester_snapshots
                           WHERE session_id = $1
                           ORDER BY day DESC
                           LIMIT 1;
                           """;

        await using var command = dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(sessionId);

        var value = await command.ExecuteScalarAsync(ct);

        if (value is not string json)
        {
            throw new InvalidOperationException(
                $"Семестр {sessionId} не найден.");
        }
        
        return JsonSerializer.Deserialize<SemesterState>(json)
            ?? throw new InvalidOperationException(
                "Не удалось прочитать состояние семестра.");
    }

    public async Task DeleteAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        const string sql = """
                           DELETE FROM semester_snapshots
                           WHERE session_id = $1;
                           """;
        
        await using var command = dataSource.CreateCommand(sql);
        
        command.Parameters.AddWithValue(sessionId);
        
        await command.ExecuteNonQueryAsync(ct);
    }
}
