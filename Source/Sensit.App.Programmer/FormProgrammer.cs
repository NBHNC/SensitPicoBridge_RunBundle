using System;
using System.Collections.Generic;
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

                UpdateProgress("Checking sensor...", 85);
                // TODO: Test and enable checking of sensors.
                //CheckSensor(sensorType);

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

                UpdateProgress("Checking sensor...", 85);
                // TODO: Test and enable checking of sensors.
                //CheckSensor(sensorType);

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

        private void CheckSensor(SensorType sensorType)
        {
            List<byte> writeData = [0x00, 0x00, 0x00];

            switch (sensorType)
            {
                case SensorType.Oxygen:
                    writeData[0] = ADS111x.AddressRegister(ADS111x.AddressPointer.ConfigRegister);
                    writeData[1] = ADS111x.ConfigRegister(
                        ADS111x.ConfigFlags.MUX_AIN0 | ADS111x.ConfigFlags.PGA_FSR_2_048V |
                        ADS111x.ConfigFlags.MODE_Continuous | ADS111x.ConfigFlags.DR_SPS_3300 |
                        ADS111x.ConfigFlags.COMP_MODE_Traditional | ADS111x.ConfigFlags.COMP_POL_Low);
                    break;
                case SensorType.HydrogenCyanide:
                case SensorType.SulfurDioxide:
                case SensorType.HydrogenSulfide:
                case SensorType.CarbonMonoxide:
                    // TODO
                    break;
                default:
                    throw new DeviceSettingNotSupportedException("Invalid sensor type.");
            }

            List<byte> readData = picoI2C.I2CWriteThenRead(I2C_ADDRESS_SENSOR, writeData, 2);
            int adcValue = (readData[0] << 8) | readData[1];
            textBoxSensorCounts.Text = adcValue.ToString(CultureInfo.InvariantCulture);

            if (adcValue < 0 || adcValue > 65535)
            {
                picoI2C.Close();
                throw new DeviceCommunicationException("Sensor ADC value out of range.");
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
        }

        #endregion
    }
}
