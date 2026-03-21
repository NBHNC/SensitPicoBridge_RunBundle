namespace Sensit.App.O2Characterizer;

public sealed class SensorRecord
{
    public long Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}

public sealed class CharacterizationRunRecord
{
    public long Id { get; set; }
    public long SensorDbId { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public DateTime RunUtc { get; set; }
    public int WarmupMinutes { get; set; }
    public int SampleCount { get; set; }
    public int SampleIntervalMs { get; set; }
    public double AverageCount { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public int Spread { get; set; }
    public double StdDev { get; set; }
    public string RunMode { get; set; } = string.Empty;
    public string RunTag { get; set; } = string.Empty;
    public double? AmbientTempC { get; set; }
    public double? AmbientHumidityPct { get; set; }
    public string PortName { get; set; } = string.Empty;
    public string AdcAddress { get; set; } = string.Empty;
    public string ConfigReadbackHex { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public sealed class CharacterizationSampleRecord
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public int SampleIndex { get; set; }
    public int RawCount { get; set; }
    public DateTime TimestampUtc { get; set; }
}

public sealed class CharacterizationResult
{
    public int WarmupMinutes { get; set; }
    public int SampleCount { get; set; }
    public int SampleIntervalMs { get; set; }
    public List<CharacterizationSampleRecord> Samples { get; set; } = new();
    public double AverageCount { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public int Spread { get; set; }
    public double StdDev { get; set; }
    public string RunMode { get; set; } = string.Empty;
    public string RunTag { get; set; } = string.Empty;
    public double? AmbientTempC { get; set; }
    public double? AmbientHumidityPct { get; set; }
    public string PortName { get; set; } = string.Empty;
    public string AdcAddress { get; set; } = string.Empty;
    public string ConfigReadbackHex { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
