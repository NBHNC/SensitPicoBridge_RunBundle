using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Sensit.TestSDK.Exceptions;

namespace Sensit.TestSDK.Devices
{
    /// <summary>
    /// I2C communication via a Raspberry Pi Pico acting as a USB-to-I2C bridge.
    ///
    /// The Pico must be flashed with the companion firmware (pico_i2c_bridge.py /
    /// main.py).  It accepts single-line text commands over its USB-CDC serial
    /// port and responds with "OK [hex bytes]" or "ERROR message".
    ///
    /// Wiring (Pico pin numbers):
    ///   GP4  (pin 6)  — SDA  → EEPROM SDA  (+ 4.7 kΩ pull-up to 3.3 V)
    ///   GP5  (pin 7)  — SCL  → EEPROM SCL  (+ 4.7 kΩ pull-up to 3.3 V)
    ///   GP22 (pin 29) — PWR  → MOSFET/transistor gate for EEPROM VCC
    ///   3V3  (pin 36) — 3.3 V supply rail (when not using switched power)
    ///   GND  (pin 38) — ground
    ///
    /// If you power the EEPROM directly from the 3.3 V rail (no switched power),
    /// the POWER ON / POWER OFF commands are accepted but have no electrical
    /// effect — the firmware simply returns "OK".
    /// </summary>
    public class PicoI2C : IDisposable
    {
        // -----------------------------------------------------------------------
        // Private state
        // -----------------------------------------------------------------------

        private SerialPort _port;
        private bool _disposed;

        /// <summary>Milliseconds to wait for a response from the Pico.</summary>
        private const int TimeoutMs = 5000;

        // -----------------------------------------------------------------------
        // Public API  (same surface as the old AardvarkI2C class)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Open the serial port to the Pico and verify the bridge firmware is running.
        /// </summary>
        /// <param name="portName">
        /// COM port name, e.g. "COM3" on Windows or "/dev/ttyACM0" on Linux.
        /// </param>
        /// <exception cref="DeviceCommunicationException">
        /// Thrown when the port cannot be opened or the Pico does not respond.
        /// </exception>
        public void Open(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                throw new DeviceCommunicationException(
                    "No COM port configured for the Pico." + Environment.NewLine +
                    "Use File → Configure Port to set the port.");

            _port = new SerialPort(portName, 115200)
            {
                ReadTimeout  = TimeoutMs,
                WriteTimeout = TimeoutMs,
                NewLine      = "\n"
            };

            try
            {
                _port.Open();
            }
            catch (Exception ex)
            {
                throw new DeviceCommunicationException(
                    $"Could not open serial port {portName}.  Is the Pico plugged in?" +
                    Environment.NewLine + ex.Message);
            }

            // Give the Pico USB-CDC stack time to settle, then flush noise.
            System.Threading.Thread.Sleep(300);
            _port.DiscardInBuffer();

            // Verify the bridge firmware is alive.
            SendCommand("PING");
            string response = ReadResponse();
            if (!response.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
            {
                Close();
                throw new DeviceCommunicationException(
                    "Pico did not respond to PING.  " +
                    "Check that pico_i2c_bridge.py is installed as main.py.");
            }
        }

        /// <summary>Close the serial port and turn off target power.</summary>
        public void Close()
        {
            if (_port?.IsOpen == true)
            {
                // Best-effort power-off; ignore errors on teardown.
                try { SendCommand("POWER OFF"); ReadResponse(); } catch { /* intentional */ }
                _port.Close();
            }
        }

        /// <summary>
        /// Write a list of bytes to an EEPROM.
        /// Data is split into 64-byte pages that match the CAT24C256 page size.
        /// </summary>
        /// <param name="i2cAddress">7-bit I2C address of the EEPROM</param>
        /// <param name="address">16-bit memory address to begin writing</param>
        /// <param name="data">bytes to write</param>
        public void EepromWrite(ushort i2cAddress, ushort address, List<byte> data)
        {
            PowerOn();
            System.Threading.Thread.Sleep(1);

            List<byte> page = [];
            foreach (byte b in data)
            {
                page.Add(b);
                if (page.Count == 64)
                {
                    page.InsertRange(0, [(byte)(address >> 8), (byte)address]);
                    I2CWrite(i2cAddress, page);
                    page.Clear();
                    address += 64;
                }
            }

            if (page.Count != 0)
            {
                page.InsertRange(0, [(byte)(address >> 8), (byte)address]);
                I2CWrite(i2cAddress, page);
            }

            PowerOff();
            System.Threading.Thread.Sleep(1);
        }

        /// <summary>
        /// Read bytes from an EEPROM, fetching in 64-byte chunks.
        /// </summary>
        /// <param name="i2cAddress">7-bit I2C address of the EEPROM</param>
        /// <param name="address">16-bit memory address to begin reading</param>
        /// <param name="length">number of bytes to read</param>
        /// <returns>list of bytes read from the EEPROM</returns>
        public List<byte> EepromRead(ushort i2cAddress, ushort address, ushort length)
        {
            PowerOn();
            System.Threading.Thread.Sleep(1);

            List<byte> eepromData = [];

            while (length >= 64)
            {
                List<byte> addr = [(byte)(address >> 8), (byte)address];
                eepromData.AddRange(I2CWriteThenRead(i2cAddress, addr, 64));
                address += 64;
                length  -= 64;
            }

            if (length > 0)
            {
                List<byte> addr = [(byte)(address >> 8), (byte)address];
                eepromData.AddRange(I2CWriteThenRead(i2cAddress, addr, length));
            }

            PowerOff();
            return eepromData;
        }

        /// <summary>
        /// Send bytes over I2C (write only).
        /// </summary>
        /// <param name="address">7-bit I2C device address</param>
        /// <param name="data">bytes to transmit</param>
        public void I2CWrite(ushort address, List<byte> data)
        {
            // Command format: WRITE <addr_hex> <b0_hex> <b1_hex> ...
            var sb = new StringBuilder($"WRITE {address:X2}");
            foreach (byte b in data)
                sb.Append($" {b:X2}");

            SendCommand(sb.ToString());
            string response = ReadResponse();

            if (!response.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                throw new DeviceCommunicationException($"Pico I2C write failed: {response}");

            // Wait for EEPROM internal write cycle (max 5 ms for CAT24C256).
            System.Threading.Thread.Sleep(10);
        }

        /// <summary>
        /// Perform an I2C write followed immediately by a read (repeated-START).
        /// </summary>
        /// <param name="address">7-bit I2C device address</param>
        /// <param name="writeData">bytes to write before the repeated START</param>
        /// <param name="readLength">number of bytes to read</param>
        /// <returns>bytes read from the device</returns>
        public List<byte> I2CWriteThenRead(ushort address, List<byte> writeData, ushort readLength)
        {
            // Command format: WRRD <addr_hex> <read_len_dec> <b0_hex> <b1_hex> ...
            var sb = new StringBuilder($"WRRD {address:X2} {readLength}");
            foreach (byte b in writeData)
                sb.Append($" {b:X2}");

            SendCommand(sb.ToString());
            string response = ReadResponse();

            if (!response.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                throw new DeviceCommandFailedException($"Pico I2C write-then-read failed: {response}");

            // Parse space-separated hex bytes after the "OK " prefix.
            string payload = response.Length > 3 ? response[3..] : string.Empty;
            string[] tokens = payload.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != readLength)
                throw new DeviceCommandFailedException(
                    $"Pico returned {tokens.Length} byte(s), expected {readLength}.");

            List<byte> result = [];
            foreach (string token in tokens)
                result.Add(Convert.ToByte(token, 16));

            System.Threading.Thread.Sleep(100);
            return result;
        }

        // -----------------------------------------------------------------------
        // Power helpers
        // -----------------------------------------------------------------------

        private void PowerOn()
        {
            SendCommand("POWER ON");
            string r = ReadResponse();
            if (!r.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                throw new DeviceCommunicationException($"Pico could not enable target power: {r}");
        }

        private void PowerOff()
        {
            SendCommand("POWER OFF");
            string r = ReadResponse();
            if (!r.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                throw new DeviceCommunicationException($"Pico could not disable target power: {r}");
        }

        // -----------------------------------------------------------------------
        // Serial helpers
        // -----------------------------------------------------------------------

        private void SendCommand(string command)
        {
            if (_port?.IsOpen != true)
                throw new DeviceCommunicationException("Pico serial port is not open.");

            _port.DiscardInBuffer();
            _port.WriteLine(command);
        }

        private string ReadResponse()
        {
            try
            {
                return _port.ReadLine().Trim();
            }
            catch (TimeoutException)
            {
                throw new DeviceCommunicationException(
                    "Pico did not respond within the timeout period.  " +
                    "Check the USB cable and that the firmware is running.");
            }
        }

        // -----------------------------------------------------------------------
        // IDisposable
        // -----------------------------------------------------------------------

        public void Dispose()
        {
            if (!_disposed)
            {
                Close();
                _port?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~PicoI2C() => Dispose();
    }
}
