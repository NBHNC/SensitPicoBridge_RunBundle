using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace Sensit.App.O2Characterizer;

public sealed class PicoBridgeClient : IDisposable
{
    private SerialPort? _serialPort;
    private readonly object _sync = new();

    public bool IsOpen => _serialPort?.IsOpen == true;

    public void Open(string portName)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            throw new ArgumentException("COM port is required.", nameof(portName));
        }

        Close();

        _serialPort = new SerialPort(portName.Trim(), 115200)
        {
            NewLine = "\n",
            ReadTimeout = 3000,
            WriteTimeout = 3000,
            Encoding = Encoding.ASCII
        };

        _serialPort.Open();
        Thread.Sleep(200);
    }

    public void Close()
    {
        if (_serialPort is null)
        {
            return;
        }

        try
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        finally
        {
            _serialPort.Dispose();
            _serialPort = null;
        }
    }

    public void Ping()
    {
        ExecuteOkOnly("PING");
    }

    public void Write(byte address, params byte[] data)
    {
        if (data is null || data.Length == 0)
        {
            throw new ArgumentException("At least one write byte is required.", nameof(data));
        }

        string command = string.Format(
            CultureInfo.InvariantCulture,
            "WRITE {0:X2} {1}",
            address,
            string.Join(" ", data.Select(static b => b.ToString("X2", CultureInfo.InvariantCulture))));

        ExecuteOkOnly(command);
    }

    public byte[] Read(byte address, int bytesToRead)
    {
        if (bytesToRead <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesToRead));
        }

        string command = string.Format(
            CultureInfo.InvariantCulture,
            "READ {0:X2} {1}",
            address,
            bytesToRead);

        return ExecuteBytes(command, bytesToRead);
    }

    public byte[] WriteThenRead(byte address, int bytesToRead, params byte[] writeData)
    {
        if (bytesToRead <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesToRead));
        }

        if (writeData is null || writeData.Length == 0)
        {
            throw new ArgumentException("At least one write byte is required.", nameof(writeData));
        }

        string command = string.Format(
            CultureInfo.InvariantCulture,
            "WRRD {0:X2} {1} {2}",
            address,
            bytesToRead,
            string.Join(" ", writeData.Select(static b => b.ToString("X2", CultureInfo.InvariantCulture))));

        return ExecuteBytes(command, bytesToRead);
    }

    private void ExecuteOkOnly(string command)
    {
        string response = SendCommand(command);
        if (!string.Equals(response, "OK", StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException($"Unexpected response to '{command}': {response}");
        }
    }

    private byte[] ExecuteBytes(string command, int expectedLength)
    {
        string response = SendCommand(command);

        if (!response.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException($"Unexpected response to '{command}': {response}");
        }

        string payload = response.Length > 2 ? response[2..].Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(payload))
        {
            if (expectedLength == 0)
            {
                return Array.Empty<byte>();
            }

            throw new IOException($"No data returned for '{command}'.");
        }

        string[] parts = payload.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != expectedLength)
        {
            throw new IOException($"Expected {expectedLength} bytes but received {parts.Length} for '{command}'.");
        }

        var data = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            data[i] = byte.Parse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return data;
    }

    private string SendCommand(string command)
    {
        if (_serialPort is null || !_serialPort.IsOpen)
        {
            throw new InvalidOperationException("Pico bridge port is not open.");
        }

        lock (_sync)
        {
            _serialPort.WriteLine(command);

            while (true)
            {
                string? response = _serialPort.ReadLine();
                if (response is null)
                {
                    continue;
                }

                response = response.Trim();
                if (string.IsNullOrWhiteSpace(response))
                {
                    continue;
                }

                if (response.StartsWith("Pico I2C Bridge ready", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return response;
            }
        }
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}
