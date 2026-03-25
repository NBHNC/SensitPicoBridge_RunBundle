using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Sensit.TestSDK.Devices;
using Sensit.TestSDK.Exceptions;
using static Sensit.App.Programmer.SensorDataLibrary;

namespace Sensit.App.Programmer
{
    // This class contains calls that are only supported on Windows.
    [SupportedOSPlatform("windows")]
    public partial class FormProgrammer : Form
    {
        private readonly ushort I2C_ADDRESS_EEPROM = 0x57; // CAT24C256 EEPROM I2C address
        private readonly ushort I2C_ADDRESS_SENSOR = 0x48; // Sensor ADC I2C address

        // ADS1115 register map / configuration used for the post-programming ADC bridge screen.
        private const byte ADS111X_REGISTER_CONVERSION = 0x00;
        private const byte ADS111X_REGISTER_CONFIG = 0x01;
        private const ushort ADS111X_CONFIG_OS_SINGLE = 0x8000;
        private const ushort ADS111X_CONFIG_MUX_AIN0_GND = 0x4000;
        private const ushort ADS111X_CONFIG_MUX_AIN1_GND = 0x5000;
        private const ushort ADS111X_CONFIG_MUX_AIN2_GND = 0x6000;
        private const ushort ADS111X_CONFIG_MUX_AIN3_GND = 0x7000;
        private const ushort ADS111X_CONFIG_PGA_4_096V = 0x0200;
        private const ushort ADS111X_CONFIG_MODE_SINGLE_SHOT = 0x0100;
        private const ushort ADS111X_CONFIG_DR_128SPS = 0x0080;
        private const ushort ADS111X_CONFIG_COMP_DISABLE = 0x0003;

        // Keep these windows intentionally loose so this is only a gross bridge / solder screen.
        private const double ADS111X_FULL_SCALE_VOLTS = 4.096;
        private const double ADC_LOW_RAIL_FAIL_VOLTS = 0.10;
        private const double ADC_HIGH_RAIL_FAIL_VOLTS = 3.15;
        private const double ADC_BIAS_TARGET_VOLTS = 1.20;
        private const double ADC_BIAS_MIN_VOLTS = 0.90;
        private const double ADC_BIAS_MAX_VOLTS = 1.50;
        private const int ADS111X_CONVERSION_DELAY_MS = 10;
        private const int ADS111X_BRIDGE_CHECK_SAMPLES = 3;


        public FormProgrammer()
        {
            InitializeComponent();

            // Inject "Configure Port..." into the existing File menu so the user
            // can pick the Pico COM port without changing the designer file.
            var configurePortItem = new ToolStripMenuItem("&Configure Port...");
            configurePortItem.Click += ConfigurePortMenuItem_Click;
            fileToolStripMenuItem.DropDownItems.Insert(0, configurePortItem);
            fileToolStripMenuItem.DropDownItems.Insert(1, new ToolStripSeparator());
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private const string WikiUrl = "https://github.com/NBHNC/SensitPicoBridge_RunBundle";

        private void WikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = WikiUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open wiki link.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Wiki",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message =
                "Smart Sensor Programmer" + Environment.NewLine + Environment.NewLine +
                "Uses a Raspberry Pi Pico as the I2C bridge for programming smart sensor EEPROMs." + Environment.NewLine +
                "Preserves the existing barcode parsing, sensor-type selection, and EEPROM record generation workflow." + Environment.NewLine + Environment.NewLine +
                $"Version: {Application.ProductVersion}";

            MessageBox.Show(
                message,
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        private void ConfigurePortMenuItem_Click(object sender, EventArgs e)
        {
            using var dlg = new Form
            {
                Text            = "Configure Pico COM Port",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition   = FormStartPosition.CenterParent,
                ClientSize      = new Size(320, 100),
                MaximizeBox     = false,
                MinimizeBox     = false
            };

            var label = new Label { Text = "Select COM port for Raspberry Pi Pico:", Location = new Point(10, 12), AutoSize = true };
            var combo = new ComboBox { Location = new Point(10, 35), Width = 180, DropDownStyle = ComboBoxStyle.DropDown };
            combo.Items.AddRange(SerialPort.GetPortNames());
            combo.Text = Properties.Settings.Default.Port ?? string.Empty;

            var btnOk     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Location = new Point(200, 33), Width = 50 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(258, 33), Width = 52 };

            dlg.Controls.AddRange([label, combo, btnOk, btnCancel]);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(combo.Text))
            {
                Properties.Settings.Default.Port = combo.Text.Trim();
                Properties.Settings.Default.Save();
            }
        }

        private void FormProgrammer_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void ButtonRead_Click(object sender, EventArgs e)
        {
            ClearStatus();
            buttonWrite.Enabled    = false;
            buttonRead.Enabled     = false;
            textBoxBarcode.Enabled = false;

            try
            {
                UpdateProgress("Connecting to Pico...", 5);
                OpenPico();

                UpdateProgress("Reading base record...", 25);
                SensorDataLibrary.SensorType sensorType = ReadBaseRecord();

                UpdateProgress("Reading device ID...", 50);
                ReadDeviceID();

                UpdateProgress("Reading manufacturing record...", 75);
                ReadManufacturingRecord();

                UpdateProgress("Running ADC bridge screen...", 85);
                RunAdcBridgeScreen(sensorType);

                UpdateProgress("Closing Pico connection...", 95);
                ClosePico();

                toolStripStatusLabel.Font      = new Font(toolStripStatusLabel.Font, FontStyle.Bold);
                toolStripStatusLabel.ForeColor = Color.Green;
                UpdateProgress("PASS", 100);
            }
            catch (Exception ex)
            {
                picoI2C.Close();

                toolStripStatusLabel.Text      = "FAIL";
                toolStripStatusLabel.Font      = new Font(toolStripStatusLabel.Font, FontStyle.Bold);
                toolStripStatusLabel.ForeColor = Color.Red;

                MessageBox.Show(ex.Message, ex.GetType().Name.ToString(CultureInfo.CurrentCulture));
            }

            buttonWrite.Enabled    = true;
            buttonRead.Enabled     = true;
            textBoxBarcode.Text    = "";
            textBoxBarcode.Enabled = true;
            textBoxBarcode.Focus();
        }

        private void ButtonWrite_Click(object sender, EventArgs e)
        {
            ClearStatus();
            buttonWrite.Enabled    = false;
            buttonRead.Enabled     = false;
            textBoxBarcode.Enabled = false;

            try
            {
                string[] words       = textBoxBarcode.Text.Split(' ');
                string serialNumber  = words[0].Split('-')[0];
                SensorDataLibrary.SensorType sensorType = ParseAlphasenseBarcode(serialNumber);

                UpdateProgress("Connecting to Pico...", 5);
                OpenPico();

                UpdateProgress("Writing base record...", 25);
                WriteBaseRecord(sensorType);

                UpdateProgress("Writing device ID...", 50);
                WriteDeviceID(sensorType, serialNumber);

                UpdateProgress("Writing manufacturing record...", 75);
                WriteManufacturingRecord(sensorType, serialNumber);

                UpdateProgress("Running ADC bridge screen...", 85);
                RunAdcBridgeScreen(sensorType);

                UpdateProgress("Closing Pico connection...", 95);
                ClosePico();

                toolStripStatusLabel.Font      = new Font(toolStripStatusLabel.Font, FontStyle.Bold);
                toolStripStatusLabel.ForeColor = Color.Green;
                UpdateProgress("PASS", 100);
            }
            catch (Exception ex)
            {
                picoI2C.Close();

                toolStripStatusLabel.Text      = "FAIL";
                toolStripStatusLabel.Font      = new Font(toolStripStatusLabel.Font, FontStyle.Bold);
                toolStripStatusLabel.ForeColor = Color.Red;

                MessageBox.Show(ex.Message, ex.GetType().Name.ToString(CultureInfo.CurrentCulture));
            }

            buttonWrite.Enabled    = true;
            buttonRead.Enabled     = true;
            textBoxBarcode.Text    = "";
            textBoxBarcode.Enabled = true;
            textBoxBarcode.Focus();
        }

        #region Programmer Commands

        readonly PicoI2C picoI2C = new();

        private void OpenPico()
        {
            picoI2C.Open(Properties.Settings.Default.Port);
        }

        private void ClosePico()
        {
            picoI2C.Close();
        }

        private SensorDataLibrary.SensorType ReadBaseRecord()
        {
            List<byte> readData = picoI2C.EepromRead(I2C_ADDRESS_EEPROM, ADDRESS_BASE_RECORD, PAGE_SIZE);

            SensorDataLibrary.BaseRecordFormat0 baseRecordFormat = new();
            baseRecordFormat.SetBytes(readData);

            switch (baseRecordFormat.SensorType)
            {
                case SensorType.Oxygen:          textBoxSensorType.Text = "O2";      break;
                case SensorType.CarbonMonoxide:  textBoxSensorType.Text = "CO";      break;
                case SensorType.HydrogenSulfide: textBoxSensorType.Text = "H2S";     break;
                case SensorType.HydrogenCyanide: textBoxSensorType.Text = "HCN";     break;
                default:
                    textBoxSensorType.Text = "Invalid";
                    picoI2C.Close();
                    throw new DeviceSettingNotSupportedException("Invalid sensor type");
            }

            return baseRecordFormat.SensorType;
        }

        private void WriteBaseRecord(SensorDataLibrary.SensorType sensorType)
        {
            List<byte> returnData = [];

            switch (sensorType)
            {
                case SensorType.Oxygen:
                    textBoxSensorType.Text = "O2";
                    SensorDataLibrary.BaseRecordFormat2 oxygenBaseRecord = new()
                    {
                        SensorRev = 1, CalScale = CAL_SCALE_OXYGEN,
                        ZeroCalibration = CAL_ZERO_OXYGEN, SensorType = SensorType.Oxygen,
                        ZeroMax = ZERO_MAX_OXYGEN, ZeroMin = ZERO_MIN_OXYGEN
                    };
                    returnData.AddRange(oxygenBaseRecord.GetBytes());
                    break;

                case SensorType.CarbonMonoxide:
                    textBoxSensorType.Text = "CO";
                    SensorDataLibrary.BaseRecordFormat0 coRecord = new()
                    {
                        SensorRev = 1, SensorType = SensorType.CarbonMonoxide,
                        CalScale = CARBONMONOXIDE_CAL_SCALE, CalPointOne = CARBONMONOXIDE_CAL_POINT_ONE
                    };
                    returnData.AddRange(coRecord.GetBytes());
                    break;

                case SensorType.HydrogenSulfide:
                    textBoxSensorType.Text = "H2S";
                    SensorDataLibrary.BaseRecordFormat0 h2sRecord = new()
                    {
                        SensorRev = 1, SensorType = SensorType.HydrogenSulfide,
                        CalScale = HYDROGENSULFIDE_CAL_SCALE
                    };
                    returnData.AddRange(h2sRecord.GetBytes());
                    break;

                case SensorType.HydrogenCyanide:
                    textBoxSensorType.Text = "HCN";
                    SensorDataLibrary.BaseRecordFormat0 hcnRecord = new()
                    {
                        SensorRev = 1, SensorType = SensorType.HydrogenCyanide,
                        CalScale = HYDROGENCYANIDE_CAL_SCALE
                    };
                    returnData.AddRange(hcnRecord.GetBytes());
                    break;

                default:
                    picoI2C.Close();
                    textBoxSensorType.Text = "Invalid";
                    throw new DeviceSettingNotSupportedException("Invalid sensor type.");
            }

            picoI2C.EepromWrite(I2C_ADDRESS_EEPROM, ADDRESS_BASE_RECORD, returnData);
        }

        private void ReadDeviceID()
        {
            List<byte> readData = picoI2C.EepromRead(I2C_ADDRESS_EEPROM, ADDRESS_DEVICE_ID, PAGE_SIZE);
            SensorDataLibrary.DeviceID deviceID = new();
            deviceID.SetBytes(readData);

            textBoxSerialNumber.Text   = deviceID.SerialNumber;
            DateTime date = new(deviceID.Year, deviceID.Month, deviceID.Day);
            textBoxDateProgrammed.Text = date.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        }

        private void WriteDeviceID(SensorDataLibrary.SensorType sensorType, string serialNumber)
        {
            string date = DateTime.Today.ToString("MMddyyyy", CultureInfo.InvariantCulture);
            textBoxDateProgrammed.Text = DateTime.Today.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);

            SensorDataLibrary.DeviceID deviceID = new()
            {
                SensorType   = sensorType,
                Year         = int.Parse(date.Substring(4, 4)),
                Month        = ushort.Parse(date[..2]),
                Day          = ushort.Parse(date.Substring(2, 2)),
                SerialNumber = serialNumber,
                RecordFormat = sensorType == SensorType.Oxygen ? (byte)2 : (byte)0
            };

            picoI2C.EepromWrite(I2C_ADDRESS_EEPROM, ADDRESS_DEVICE_ID, deviceID.GetBytes());
        }

        private void ReadManufacturingRecord()
        {
            picoI2C.EepromRead(I2C_ADDRESS_EEPROM, ADDRESS_MANUFACTURING_ID, PAGE_SIZE);
        }

        private void WriteManufacturingRecord(SensorDataLibrary.SensorType sensorType, string serialNumber)
        {
            string date = DateTime.Today.ToString("MMddyyyy", CultureInfo.InvariantCulture);

            SensorDataLibrary.ManufactureID manufactureID = new()
            {
                SensorType   = sensorType,
                Year         = int.Parse(date.Substring(4, 4)),
                Month        = ushort.Parse(date[..2]),
                Day          = ushort.Parse(date.Substring(2, 2)),
                SerialNumber = serialNumber,
                RecordFormat = sensorType == SensorType.Oxygen ? (byte)2 : (byte)0
            };

            textBoxSerialNumber.Text += Environment.NewLine + serialNumber;
            picoI2C.EepromWrite(I2C_ADDRESS_EEPROM, ADDRESS_MANUFACTURING_ID, manufactureID.GetBytes());
        }

        private void RunAdcBridgeScreen(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Oxygen:
                    RunOxygenAdcBridgeScreen();
                    break;

                case SensorType.HydrogenCyanide:
                case SensorType.HydrogenSulfide:
                case SensorType.CarbonMonoxide:
                    RunBiasedSensorAdcBridgeScreen();
                    break;

                default:
                    throw new DeviceSettingNotSupportedException("ADC bridge screen is not supported for this sensor type.");
            }
        }

        private void RunOxygenAdcBridgeScreen()
        {
            int ain0Counts = ReadAds111xSingleEndedAverage(0);
            int ain1Counts = ReadAds111xSingleEndedAverage(1);

            double ain0Volts = ConvertAds111xCountsToVolts(ain0Counts);
            double ain1Volts = ConvertAds111xCountsToVolts(ain1Counts);

            textBoxSensorCounts.Text =
                $"AIN0={ain0Counts} ({ain0Volts:F3} V){Environment.NewLine}" +
                $"AIN1={ain1Counts} ({ain1Volts:F3} V)";

            // O2 board:
            // - AIN1 is the known 1.2 V bias node and should drive pass/fail.
            // - AIN0 is the live sensor output (Vout), so for now we only display/log it.
            ValidateAdcBiasNode("AIN1", ain1Volts);
        }

        private void RunBiasedSensorAdcBridgeScreen()
        {
            int ain0Counts = ReadAds111xSingleEndedAverage(0);
            int ain1Counts = ReadAds111xSingleEndedAverage(1);
            int ain2Counts = ReadAds111xSingleEndedAverage(2);
            int ain3Counts = ReadAds111xSingleEndedAverage(3);

            double ain0Volts = ConvertAds111xCountsToVolts(ain0Counts);
            double ain1Volts = ConvertAds111xCountsToVolts(ain1Counts);
            double ain2Volts = ConvertAds111xCountsToVolts(ain2Counts);
            double ain3Volts = ConvertAds111xCountsToVolts(ain3Counts);

            textBoxSensorCounts.Text =
                $"AIN0={ain0Counts} ({ain0Volts:F3} V), AIN1={ain1Counts} ({ain1Volts:F3} V){Environment.NewLine}" +
                $"AIN2={ain2Counts} ({ain2Volts:F3} V), AIN3={ain3Counts} ({ain3Volts:F3} V)";

            ValidateAdcInputNotOnRail("AIN0", ain0Volts);
            ValidateAdcBiasNode("AIN1", ain1Volts);
            ValidateAdcInputNotOnRail("AIN2", ain2Volts);
            ValidateAdcBiasNode("AIN3", ain3Volts);
        }

        private int ReadAds111xSingleEndedAverage(int channel)
        {
            int total = 0;

            for (int i = 0; i < ADS111X_BRIDGE_CHECK_SAMPLES; i++)
            {
                total += ReadAds111xSingleEndedSample(channel);
            }

            return total / ADS111X_BRIDGE_CHECK_SAMPLES;
        }

        private short ReadAds111xSingleEndedSample(int channel)
        {
            ushort mux = channel switch
            {
                0 => ADS111X_CONFIG_MUX_AIN0_GND,
                1 => ADS111X_CONFIG_MUX_AIN1_GND,
                2 => ADS111X_CONFIG_MUX_AIN2_GND,
                3 => ADS111X_CONFIG_MUX_AIN3_GND,
                _ => throw new DeviceSettingNotSupportedException("Unsupported ADC channel for bridge screen.")
            };

            ushort config = (ushort)(ADS111X_CONFIG_OS_SINGLE |
                                     mux |
                                     ADS111X_CONFIG_PGA_4_096V |
                                     ADS111X_CONFIG_MODE_SINGLE_SHOT |
                                     ADS111X_CONFIG_DR_128SPS |
                                     ADS111X_CONFIG_COMP_DISABLE);

            List<byte> configWriteData =
            [
                ADS111X_REGISTER_CONFIG,
                (byte)(config >> 8),
                (byte)(config & 0xFF)
            ];

            // Write config and confirm the ADC acknowledges the transaction.
            picoI2C.I2CWriteThenRead(I2C_ADDRESS_SENSOR, configWriteData, 2);

            System.Threading.Thread.Sleep(ADS111X_CONVERSION_DELAY_MS);

            List<byte> readData = picoI2C.I2CWriteThenRead(
                I2C_ADDRESS_SENSOR,
                [ADS111X_REGISTER_CONVERSION],
                2);

            if (readData.Count != 2)
            {
                throw new DeviceCommunicationException("ADC bridge screen failed: invalid conversion response length.");
            }

            ushort rawCounts = (ushort)((readData[0] << 8) | readData[1]);
            return unchecked((short)rawCounts);
        }

        private static double ConvertAds111xCountsToVolts(int counts)
        {
            if (counts < 0)
            {
                throw new TestException($"ADC bridge screen failed: negative single-ended reading ({counts} counts).");
            }

            return counts * ADS111X_FULL_SCALE_VOLTS / 32768.0;
        }

        private static void ValidateAdcInputNotOnRail(string channelName, double volts)
        {
            if (volts <= ADC_LOW_RAIL_FAIL_VOLTS)
            {
                throw new TestException(
                    $"ADC bridge screen failed: {channelName} appears shorted near GND ({volts:F3} V).");
            }

            if (volts >= ADC_HIGH_RAIL_FAIL_VOLTS)
            {
                throw new TestException(
                    $"ADC bridge screen failed: {channelName} appears shorted near VDD ({volts:F3} V).");
            }
        }

        private static void ValidateAdcBiasNode(string channelName, double volts)
        {
            if (volts < ADC_BIAS_MIN_VOLTS || volts > ADC_BIAS_MAX_VOLTS)
            {
                throw new TestException(
                    $"ADC bridge screen failed: {channelName} bias node is out of range ({volts:F3} V, expected about {ADC_BIAS_TARGET_VOLTS:F1} V).");
            }
        }

        #endregion

        #region Helper Methods

        private static SensorDataLibrary.SensorType ParseAlphasenseBarcode(string serialNumber)
        {
            int numDigits = serialNumber.Length;
            uint sensorTypeCode;

            if      (numDigits >= 10) sensorTypeCode = uint.Parse(serialNumber[..4]);
            else if (numDigits == 9)  sensorTypeCode = uint.Parse(serialNumber[..3]);
            else if (numDigits == 8)  sensorTypeCode = uint.Parse(serialNumber[..2]);
            else throw new TestException("Invalid serial number format.");

            return sensorTypeCode switch
            {
                185            => SensorType.Oxygen,
                20 or 21 or 22 => SensorType.HydrogenSulfide,
                11 or 15       => SensorType.CarbonMonoxide,
                55             => SensorType.HydrogenCyanide,
                51             => throw new TestException("SO2 sensors are not supported yet."),
                _              => throw new TestException("Unknown sensor type.")
            };
        }

        private void UpdateProgress(string message, int progress)
        {
            toolStripStatusLabel.Text  = message;
            toolStripProgressBar.Value = progress;
            Update();
        }

        private void ClearStatus()
        {
            textBoxBarcode.Enabled = false;
            toolStripStatusLabel.Font      = new Font(toolStripStatusLabel.Font, FontStyle.Regular);
            toolStripStatusLabel.ForeColor = SystemColors.ControlText;
            textBoxSerialNumber.Text   = string.Empty;
            textBoxSensorType.Text     = string.Empty;
            textBoxDateProgrammed.Text = string.Empty;
            textBoxSensorCounts.Text   = string.Empty;
        }

        #endregion
    }
}
