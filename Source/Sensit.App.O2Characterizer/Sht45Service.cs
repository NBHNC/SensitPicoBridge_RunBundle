using System.Globalization;

namespace Sensit.App.O2Characterizer;

public sealed class Sht45Service
{
    private const byte SensorAddress = 0x44;
    private const byte MeasureHighPrecisionNoHeater = 0xFD;
    private const int MeasureDelayMs = 20;
    private const int DefaultReadCount = 3;
    private const int DefaultInterReadDelayMs = 75;

    public Task<Sht45Reading> ReadAmbientAsync(
        string portName,
        int readCount = DefaultReadCount,
        int interReadDelayMs = DefaultInterReadDelayMs,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ReadAmbient(portName, readCount, interReadDelayMs, cancellationToken), cancellationToken);
    }

    private static Sht45Reading ReadAmbient(
        string portName,
        int readCount,
        int interReadDelayMs,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            throw new ArgumentException("COM port is required for SHT45 reads.", nameof(portName));
        }

        if (readCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(readCount));
        }

        using var bridge = new PicoBridgeClient();
        bridge.Open(portName);
        bridge.Ping();

        var temperatureValues = new List<double>(readCount);
        var humidityValues = new List<double>(readCount);

        for (int i = 0; i < readCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bridge.Write(SensorAddress, MeasureHighPrecisionNoHeater);
            Thread.Sleep(MeasureDelayMs);

            byte[] payload = bridge.Read(SensorAddress, 6);
            if (payload.Length != 6)
            {
                throw new InvalidOperationException($"SHT45 returned {payload.Length} bytes instead of 6.");
            }

            ValidateCrc(payload[0], payload[1], payload[2], "temperature");
            ValidateCrc(payload[3], payload[4], payload[5], "humidity");

            ushort rawTemperature = (ushort)((payload[0] << 8) | payload[1]);
            ushort rawHumidity = (ushort)((payload[3] << 8) | payload[4]);

            double temperatureC = -45.0 + (175.0 * rawTemperature / 65535.0);
            double relativeHumidityPct = -6.0 + (125.0 * rawHumidity / 65535.0);
            relativeHumidityPct = Math.Clamp(relativeHumidityPct, 0.0, 100.0);

            temperatureValues.Add(temperatureC);
            humidityValues.Add(relativeHumidityPct);

            if (i < readCount - 1)
            {
                Thread.Sleep(Math.Max(1, interReadDelayMs));
            }
        }

        return new Sht45Reading
        {
            TemperatureC = temperatureValues.Average(),
            RelativeHumidityPct = humidityValues.Average(),
            PortName = portName.Trim(),
            AddressHex = $"0x{SensorAddress:X2}",
            ReadCount = readCount,
            Notes = $"Averaged {readCount} SHT45 read(s) from {portName.Trim()} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)} UTC."
        };
    }

    private static void ValidateCrc(byte msb, byte lsb, byte expectedCrc, string fieldName)
    {
        byte[] bytes = [msb, lsb];
        byte actualCrc = ComputeCrc8(bytes);
        if (actualCrc != expectedCrc)
        {
            throw new InvalidOperationException(
                $"SHT45 {fieldName} CRC mismatch. Expected {expectedCrc:X2}, calculated {actualCrc:X2}.");
        }
    }

    private static byte ComputeCrc8(ReadOnlySpan<byte> data)
    {
        byte crc = 0xFF;

        foreach (byte value in data)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (byte)((crc & 0x80) != 0 ? (crc << 1) ^ 0x31 : crc << 1);
            }
        }

        return crc;
    }
}

public sealed class Sht45Reading
{
    public double TemperatureC { get; set; }
    public double RelativeHumidityPct { get; set; }
    public string PortName { get; set; } = string.Empty;
    public string AddressHex { get; set; } = string.Empty;
    public int ReadCount { get; set; }
    public string Notes { get; set; } = string.Empty;
}
