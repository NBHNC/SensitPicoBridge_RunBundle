using Microsoft.Data.Sqlite;

namespace Sensit.App.O2Characterizer;

public sealed class DatabaseService
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        string appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sensit",
            "O2Characterizer");

        Directory.CreateDirectory(appDataFolder);

        _databasePath = Path.Combine(appDataFolder, "o2_characterizer.db");
        _connectionString = $"Data Source={_databasePath}";
    }

    public string DatabasePath => _databasePath;

    public void EnsureDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS sensors
(
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    sensor_id TEXT NOT NULL,
    notes TEXT NOT NULL,
    created_utc TEXT NOT NULL
);
";
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS characterization_runs
(
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
    run_mode TEXT NOT NULL DEFAULT '',
    run_tag TEXT NOT NULL DEFAULT '',
    ambient_temp_c REAL NULL,
    ambient_humidity_pct REAL NULL,
    port_name TEXT NOT NULL DEFAULT '',
    adc_address TEXT NOT NULL DEFAULT '',
    config_readback_hex TEXT NOT NULL DEFAULT '',
    notes TEXT NOT NULL,
    FOREIGN KEY(sensor_db_id) REFERENCES sensors(id)
);
";
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS characterization_samples
(
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    sample_index INTEGER NOT NULL,
    raw_count INTEGER NOT NULL,
    timestamp_utc TEXT NOT NULL,
    FOREIGN KEY(run_id) REFERENCES characterization_runs(id)
);
";
            command.ExecuteNonQuery();
        }

        EnsureColumnExists(connection, "characterization_runs", "run_mode", "TEXT NOT NULL DEFAULT ''");
        EnsureColumnExists(connection, "characterization_runs", "run_tag", "TEXT NOT NULL DEFAULT ''");
        EnsureColumnExists(connection, "characterization_runs", "ambient_temp_c", "REAL NULL");
        EnsureColumnExists(connection, "characterization_runs", "ambient_humidity_pct", "REAL NULL");
        EnsureColumnExists(connection, "characterization_runs", "port_name", "TEXT NOT NULL DEFAULT ''");
        EnsureColumnExists(connection, "characterization_runs", "adc_address", "TEXT NOT NULL DEFAULT ''");
        EnsureColumnExists(connection, "characterization_runs", "config_readback_hex", "TEXT NOT NULL DEFAULT ''");
    }

    private static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string definition)
    {
        using var pragma = connection.CreateCommand();
        pragma.CommandText = $"PRAGMA table_info({tableName});";

        bool found = false;
        using (var reader = pragma.ExecuteReader())
        {
            while (reader.Read())
            {
                string existingColumnName = reader.GetString(1);
                if (string.Equals(existingColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }
        }

        if (found)
        {
            return;
        }

        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};";
        alter.ExecuteNonQuery();
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
       COALESCE(r.run_mode, ''),
       COALESCE(r.run_tag, ''),
       r.ambient_temp_c,
       r.ambient_humidity_pct,
       COALESCE(r.port_name, ''),
       COALESCE(r.adc_address, ''),
       COALESCE(r.config_readback_hex, ''),
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
                RunMode = reader.GetString(12),
                RunTag = reader.GetString(13),
                AmbientTempC = reader.IsDBNull(14) ? null : reader.GetDouble(14),
                AmbientHumidityPct = reader.IsDBNull(15) ? null : reader.GetDouble(15),
                PortName = reader.GetString(16),
                AdcAddress = reader.GetString(17),
                ConfigReadbackHex = reader.GetString(18),
                Notes = reader.GetString(19)
            });
        }

        return list;
    }

    public List<CharacterizationSampleRecord> GetSamplesForRun(long runId)
    {
        var list = new List<CharacterizationSampleRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, run_id, sample_index, raw_count, timestamp_utc
FROM characterization_samples
WHERE run_id = $run_id
ORDER BY sample_index;";
        command.Parameters.AddWithValue("$run_id", runId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new CharacterizationSampleRecord
            {
                Id = reader.GetInt64(0),
                RunId = reader.GetInt64(1),
                SampleIndex = reader.GetInt32(2),
                RawCount = reader.GetInt32(3),
                TimestampUtc = DateTime.Parse(reader.GetString(4)).ToUniversalTime()
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
    run_mode,
    run_tag,
    ambient_temp_c,
    ambient_humidity_pct,
    port_name,
    adc_address,
    config_readback_hex,
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
    $run_mode,
    $run_tag,
    $ambient_temp_c,
    $ambient_humidity_pct,
    $port_name,
    $adc_address,
    $config_readback_hex,
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
            runCommand.Parameters.AddWithValue("$run_mode", result.RunMode ?? string.Empty);
            runCommand.Parameters.AddWithValue("$run_tag", result.RunTag ?? string.Empty);
            runCommand.Parameters.AddWithValue("$ambient_temp_c", (object?)result.AmbientTempC ?? DBNull.Value);
            runCommand.Parameters.AddWithValue("$ambient_humidity_pct", (object?)result.AmbientHumidityPct ?? DBNull.Value);
            runCommand.Parameters.AddWithValue("$port_name", result.PortName ?? string.Empty);
            runCommand.Parameters.AddWithValue("$adc_address", result.AdcAddress ?? string.Empty);
            runCommand.Parameters.AddWithValue("$config_readback_hex", result.ConfigReadbackHex ?? string.Empty);
            runCommand.Parameters.AddWithValue("$notes", result.Notes ?? string.Empty);

            runId = (long)(runCommand.ExecuteScalar() ?? 0L);
        }

        foreach (CharacterizationSampleRecord sample in result.Samples)
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
            sampleCommand.Parameters.AddWithValue("$sample_index", sample.SampleIndex);
            sampleCommand.Parameters.AddWithValue("$raw_count", sample.RawCount);
            sampleCommand.Parameters.AddWithValue("$timestamp_utc", sample.TimestampUtc.ToString("o"));
            sampleCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        return runId;
    }
}
