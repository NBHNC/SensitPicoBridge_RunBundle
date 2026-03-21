using Microsoft.Data.Sqlite;

namespace Sensit.App.O2Characterizer;

public sealed class DatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseService(string? dbPath = null)
    {
        string baseFolder = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(baseFolder);

        _dbPath = dbPath ?? Path.Combine(baseFolder, "o2_characterization.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    public string DatabasePath => _dbPath;

    public void EnsureDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS sensors (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    sensor_id TEXT NOT NULL UNIQUE,
    notes TEXT NOT NULL DEFAULT '',
    created_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS characterization_runs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    sensor_db_id INTEGER NOT NULL,
    run_utc TEXT NOT NULL,
    warmup_minutes INTEGER NOT NULL,
    sample_count INTEGER NOT NULL,
    sample_interval_ms INTEGER NOT NULL,
    average_count REAL NOT NULL,
    min_count INTEGER NOT NULL,
    max_count INTEGER NOT NULL,
    spread INTEGER NOT NULL,
    std_dev REAL NOT NULL,
    notes TEXT NOT NULL DEFAULT '',
    FOREIGN KEY(sensor_db_id) REFERENCES sensors(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS characterization_samples (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    sample_index INTEGER NOT NULL,
    raw_count INTEGER NOT NULL,
    timestamp_utc TEXT NOT NULL,
    FOREIGN KEY(run_id) REFERENCES characterization_runs(id) ON DELETE CASCADE
);
";
        command.ExecuteNonQuery();
    }

    public long AddSensor(string sensorId, string notes)
    {
        if (string.IsNullOrWhiteSpace(sensorId))
        {
            throw new ArgumentException("Sensor ID is required.", nameof(sensorId));
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO sensors (sensor_id, notes, created_utc)
VALUES ($sensor_id, $notes, $created_utc);
SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$sensor_id", sensorId.Trim());
        command.Parameters.AddWithValue("$notes", notes?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$created_utc", DateTime.UtcNow.ToString("o"));

        return (long)(command.ExecuteScalar() ?? 0L);
    }

    public List<SensorRecord> GetSensors()
    {
        var list = new List<SensorRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, sensor_id, notes, created_utc
FROM sensors
ORDER BY sensor_id;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new SensorRecord
            {
                Id = reader.GetInt64(0),
                SensorId = reader.GetString(1),
                Notes = reader.GetString(2),
                CreatedUtc = DateTime.Parse(reader.GetString(3)).ToUniversalTime()
            });
        }

        return list;
    }

    public List<CharacterizationRunRecord> GetRunsForSensor(long sensorDbId)
    {
        var list = new List<CharacterizationRunRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT r.id,
       r.sensor_db_id,
       s.sensor_id,
       r.run_utc,
       r.warmup_minutes,
       r.sample_count,
       r.sample_interval_ms,
       r.average_count,
       r.min_count,
       r.max_count,
       r.spread,
       r.std_dev,
       r.notes
FROM characterization_runs r
INNER JOIN sensors s ON s.id = r.sensor_db_id
WHERE r.sensor_db_id = $sensor_db_id
ORDER BY r.run_utc DESC;";
        command.Parameters.AddWithValue("$sensor_db_id", sensorDbId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new CharacterizationRunRecord
            {
                Id = reader.GetInt64(0),
                SensorDbId = reader.GetInt64(1),
                SensorId = reader.GetString(2),
                RunUtc = DateTime.Parse(reader.GetString(3)).ToUniversalTime(),
                WarmupMinutes = reader.GetInt32(4),
                SampleCount = reader.GetInt32(5),
                SampleIntervalMs = reader.GetInt32(6),
                AverageCount = reader.GetDouble(7),
                MinCount = reader.GetInt32(8),
                MaxCount = reader.GetInt32(9),
                Spread = reader.GetInt32(10),
                StdDev = reader.GetDouble(11),
                Notes = reader.GetString(12)
            });
        }

        return list;
    }

    public long SaveRun(long sensorDbId, CharacterizationResult result)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        long runId;

        using (var runCommand = connection.CreateCommand())
        {
            runCommand.Transaction = transaction;
            runCommand.CommandText = @"
INSERT INTO characterization_runs (
    sensor_db_id,
    run_utc,
    warmup_minutes,
    sample_count,
    sample_interval_ms,
    average_count,
    min_count,
    max_count,
    spread,
    std_dev,
    notes)
VALUES (
    $sensor_db_id,
    $run_utc,
    $warmup_minutes,
    $sample_count,
    $sample_interval_ms,
    $average_count,
    $min_count,
    $max_count,
    $spread,
    $std_dev,
    $notes);
SELECT last_insert_rowid();";

            runCommand.Parameters.AddWithValue("$sensor_db_id", sensorDbId);
            runCommand.Parameters.AddWithValue("$run_utc", DateTime.UtcNow.ToString("o"));
            runCommand.Parameters.AddWithValue("$warmup_minutes", result.WarmupMinutes);
            runCommand.Parameters.AddWithValue("$sample_count", result.SampleCount);
            runCommand.Parameters.AddWithValue("$sample_interval_ms", result.SampleIntervalMs);
            runCommand.Parameters.AddWithValue("$average_count", result.AverageCount);
            runCommand.Parameters.AddWithValue("$min_count", result.MinCount);
            runCommand.Parameters.AddWithValue("$max_count", result.MaxCount);
            runCommand.Parameters.AddWithValue("$spread", result.Spread);
            runCommand.Parameters.AddWithValue("$std_dev", result.StdDev);
            runCommand.Parameters.AddWithValue("$notes", result.Notes ?? string.Empty);

            runId = (long)(runCommand.ExecuteScalar() ?? 0L);
        }

        for (int i = 0; i < result.Samples.Count; i++)
        {
            using var sampleCommand = connection.CreateCommand();
            sampleCommand.Transaction = transaction;
            sampleCommand.CommandText = @"
INSERT INTO characterization_samples (
    run_id,
    sample_index,
    raw_count,
    timestamp_utc)
VALUES (
    $run_id,
    $sample_index,
    $raw_count,
    $timestamp_utc);";
            sampleCommand.Parameters.AddWithValue("$run_id", runId);
            sampleCommand.Parameters.AddWithValue("$sample_index", i + 1);
            sampleCommand.Parameters.AddWithValue("$raw_count", result.Samples[i]);
            sampleCommand.Parameters.AddWithValue("$timestamp_utc", DateTime.UtcNow.ToString("o"));
            sampleCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        return runId;
    }
}
