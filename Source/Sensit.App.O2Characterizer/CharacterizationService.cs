namespace Sensit.App.O2Characterizer;

public sealed class CharacterizationService
{
    private const byte SensorAdcAddress = 0x48;
    private const byte AdsPointerConversion = 0x00;
    private const byte AdsPointerConfig = 0x01;

    // Same simple ADS111x configuration used in the programmer ADC test.
    // AIN0 single-ended, +/-2.048 V, continuous mode, 128 SPS, comparator disabled.
    private const byte AdsConfigMsb = 0x44;
    private const byte AdsConfigLsb = 0x83;
    private const int AdsSettleMs = 25;

    private readonly Random _random = new();

    public async Task<CharacterizationResult> RunSimulatedAmbientO2Async(
        int warmupMinutes,
        int sampleCount,
        int sampleIntervalMs,
        CancellationToken cancellationToken = default)
    {
        const int nominalCount = 15250;
        const int driftWindow = 80;
        const int noiseWindow = 25;

        var samples = new List<CharacterizationSampleRecord>(sampleCount);
        int baseline = nominalCount + _random.Next(-driftWindow, driftWindow + 1);

        for (int i = 0; i < sampleCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            baseline += _random.Next(-4, 5);
            int sample = baseline + _random.Next(-noiseWindow, noiseWindow + 1);
            samples.Add(new CharacterizationSampleRecord
            {
                SampleIndex = i + 1,
                RawCount = sample,
                TimestampUtc = DateTime.UtcNow
            });

            if (i < sampleCount - 1)
            {
                await Task.Delay(sampleIntervalMs, cancellationToken);
            }
        }

        CharacterizationResult result = BuildResult(samples, warmupMinutes, sampleIntervalMs);
        result.RunMode = "Simulated";
        result.AdcAddress = $"0x{SensorAdcAddress:X2}";
        result.Notes = "Simulated ambient-air run. Replace with live ADC reads when ready.";
        return result;
    }

    public Task<CharacterizationResult> RunLiveAmbientO2Async(
        string portName,
        int warmupMinutes,
        int sampleCount,
        int sampleIntervalMs,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => RunLiveAmbientO2(portName, warmupMinutes, sampleCount, sampleIntervalMs, cancellationToken), cancellationToken);
    }

    private CharacterizationResult RunLiveAmbientO2(
        string portName,
        int warmupMinutes,
        int sampleCount,
        int sampleIntervalMs,
        CancellationToken cancellationToken)
    {
        using var bridge = new PicoBridgeClient();
        bridge.Open(portName);
        bridge.Ping();

        byte[] configReadback = bridge.WriteThenRead(SensorAdcAddress, 2, AdsPointerConfig, AdsConfigMsb, AdsConfigLsb);
        if (configReadback.Length != 2)
        {
            throw new InvalidOperationException("ADC config readback returned the wrong number of bytes.");
        }

        if (configReadback[0] != AdsConfigMsb || configReadback[1] != AdsConfigLsb)
        {
            throw new InvalidOperationException(
                $"ADC config readback mismatch. Expected {AdsConfigMsb:X2} {AdsConfigLsb:X2}, got {configReadback[0]:X2} {configReadback[1]:X2}.");
        }

        Thread.Sleep(AdsSettleMs);

        var samples = new List<CharacterizationSampleRecord>(sampleCount);
        for (int i = 0; i < sampleCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] conversion = bridge.WriteThenRead(SensorAdcAddress, 2, AdsPointerConversion);
            if (conversion.Length != 2)
            {
                throw new InvalidOperationException("ADC conversion read returned the wrong number of bytes.");
            }

            short signedValue = unchecked((short)((conversion[0] << 8) | conversion[1]));
            samples.Add(new CharacterizationSampleRecord
            {
                SampleIndex = i + 1,
                RawCount = signedValue,
                TimestampUtc = DateTime.UtcNow
            });

            if (i < sampleCount - 1)
            {
                Thread.Sleep(Math.Max(1, sampleIntervalMs));
            }
        }

        CharacterizationResult result = BuildResult(samples, warmupMinutes, sampleIntervalMs);
        result.RunMode = "Live ADC";
        result.PortName = portName.Trim();
        result.AdcAddress = $"0x{SensorAdcAddress:X2}";
        result.ConfigReadbackHex = $"{configReadback[0]:X2} {configReadback[1]:X2}";
        result.Notes = $"Live ADC run via {result.PortName}. Assumes the board was pre-warmed for {warmupMinutes} minute(s) before characterization.";
        return result;
    }

    public CharacterizationResult BuildResult(
        IReadOnlyList<CharacterizationSampleRecord> samples,
        int warmupMinutes,
        int sampleIntervalMs)
    {
        if (samples.Count == 0)
        {
            throw new ArgumentException("At least one sample is required.", nameof(samples));
        }

        List<int> counts = samples.Select(static s => s.RawCount).ToList();
        double average = counts.Average();
        int min = counts.Min();
        int max = counts.Max();
        int spread = max - min;
        double variance = counts.Select(v => Math.Pow(v - average, 2)).Average();
        double stdDev = Math.Sqrt(variance);

        return new CharacterizationResult
        {
            WarmupMinutes = warmupMinutes,
            SampleCount = samples.Count,
            SampleIntervalMs = sampleIntervalMs,
            Samples = samples.Select(static s => new CharacterizationSampleRecord
            {
                SampleIndex = s.SampleIndex,
                RawCount = s.RawCount,
                TimestampUtc = s.TimestampUtc
            }).ToList(),
            AverageCount = average,
            MinCount = min,
            MaxCount = max,
            Spread = spread,
            StdDev = stdDev
        };
    }
}
