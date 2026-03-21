using System.Diagnostics;

namespace Sensit.App.O2Characterizer;

public sealed class CharacterizationService
{
    private readonly Random _random = new();

    public async Task<CharacterizationResult> RunSimulatedAmbientO2Async(
        int warmupMinutes,
        int sampleCount,
        int sampleIntervalMs,
        CancellationToken cancellationToken = default)
    {
        // Starter implementation: simulate a warmed-up ambient O2 sensor.
        // Replace this later with live ADC reads.
        const int nominalCount = 15250;
        const int driftWindow = 80;
        const int noiseWindow = 25;

        var samples = new List<int>(sampleCount);
        int baseline = nominalCount + _random.Next(-driftWindow, driftWindow + 1);

        for (int i = 0; i < sampleCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // A little random walk + small noise to look like a real sensor.
            baseline += _random.Next(-4, 5);
            int sample = baseline + _random.Next(-noiseWindow, noiseWindow + 1);
            samples.Add(sample);

            if (i < sampleCount - 1)
            {
                await Task.Delay(sampleIntervalMs, cancellationToken);
            }
        }

        return BuildResult(samples, warmupMinutes, sampleIntervalMs);
    }

    public CharacterizationResult BuildResult(IReadOnlyList<int> samples, int warmupMinutes, int sampleIntervalMs)
    {
        if (samples.Count == 0)
        {
            throw new ArgumentException("At least one sample is required.", nameof(samples));
        }

        double average = samples.Average();
        int min = samples.Min();
        int max = samples.Max();
        int spread = max - min;
        double variance = samples.Select(v => Math.Pow(v - average, 2)).Average();
        double stdDev = Math.Sqrt(variance);

        return new CharacterizationResult
        {
            WarmupMinutes = warmupMinutes,
            SampleCount = samples.Count,
            SampleIntervalMs = sampleIntervalMs,
            Samples = samples.ToList(),
            AverageCount = average,
            MinCount = min,
            MaxCount = max,
            Spread = spread,
            StdDev = stdDev,
            Notes = "Simulated ambient-air run. Replace with real ADC reads later."
        };
    }
}
