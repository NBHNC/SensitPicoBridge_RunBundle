using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace Sensit.App.O2Characterizer;

public partial class Form1 : Form
{
    private readonly DatabaseService _database;
    private readonly CharacterizationService _characterizationService;
    private readonly BindingSource _sensorBindingSource = new();
    private readonly BindingSource _runBindingSource = new();
    private readonly BindingSource _sampleBindingSource = new();
    private readonly ToolTip _toolTip = new();

    private Label? _labelRunTag;
    private ComboBox? _comboBoxRunTag;
    private Label? _labelAmbientTempC;
    private TextBox? _textBoxAmbientTempC;
    private Label? _labelAmbientHumidityPct;
    private TextBox? _textBoxAmbientHumidityPct;
    private Label? _labelTrendInfo;
    private Panel? _panelTrend;
    private CheckBox? _checkBoxAutoReadSht45;

    private readonly Sht45Service _sht45Service = new();

    public Form1()
    {
        InitializeComponent();
        InitializeExtendedControls();
        ApplyLayoutFixes();
        Resize += (_, _) => ApplyLayoutFixes();

        _database = new DatabaseService();
        _characterizationService = new CharacterizationService();

        dataGridViewSensors.AutoGenerateColumns = true;
        dataGridViewRuns.AutoGenerateColumns = true;
        dataGridViewSamples.AutoGenerateColumns = true;

        dataGridViewSensors.DataSource = _sensorBindingSource;
        dataGridViewRuns.DataSource = _runBindingSource;
        dataGridViewSamples.DataSource = _sampleBindingSource;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        try
        {
            _database.EnsureDatabase();
            labelDatabasePathValue.Text = _database.DatabasePath;

            numericUpDownWarmupMinutes.Value = 5;
            numericUpDownSampleCount.Value = 30;
            numericUpDownSampleIntervalMs.Value = 100;

            if (_comboBoxRunTag is not null && _comboBoxRunTag.Items.Count > 0)
            {
                _comboBoxRunTag.Text = "Engineering Sample";
            }

            RefreshPorts();
            RefreshSensors();
            UpdateLiveControlsState();
            UpdateTrend();
            UpdateStatus("Ready.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Startup failed.");
        }
    }

    private void InitializeExtendedControls()
    {
        _labelRunTag = new Label
        {
            AutoSize = true,
            Location = new Point(19, 96),
            Name = "labelRunTagDynamic",
            Size = new Size(49, 15),
            Text = "Run Tag"
        };

        _comboBoxRunTag = new ComboBox
        {
            Location = new Point(19, 116),
            Name = "comboBoxRunTagDynamic",
            Size = new Size(150, 23),
            DropDownStyle = ComboBoxStyle.DropDown
        };
        _comboBoxRunTag.Items.AddRange(new object[]
        {
            "Known Good",
            "Engineering Sample",
            "Repeatability Test",
            "Warmup Study",
            "Suspect",
            "Retest"
        });

        _labelAmbientTempC = new Label
        {
            AutoSize = true,
            Location = new Point(185, 96),
            Name = "labelAmbientTempCDynamic",
            Size = new Size(54, 15),
            Text = "Temp °C"
        };

        _textBoxAmbientTempC = new TextBox
        {
            Location = new Point(185, 116),
            Name = "textBoxAmbientTempCDynamic",
            Size = new Size(78, 23)
        };

        _labelAmbientHumidityPct = new Label
        {
            AutoSize = true,
            Location = new Point(279, 96),
            Name = "labelAmbientHumidityPctDynamic",
            Size = new Size(35, 15),
            Text = "RH %"
        };

        _textBoxAmbientHumidityPct = new TextBox
        {
            Location = new Point(279, 116),
            Name = "textBoxAmbientHumidityPctDynamic",
            Size = new Size(78, 23)
        };

        _checkBoxAutoReadSht45 = new CheckBox
        {
            AutoSize = true,
            Location = new Point(373, 118),
            Name = "checkBoxAutoReadSht45Dynamic",
            Size = new Size(114, 19),
            Text = "Auto-read SHT45",
            Checked = false
        };
        _checkBoxAutoReadSht45.CheckedChanged += (_, _) => UpdateLiveControlsState();

        _toolTip.SetToolTip(_comboBoxRunTag, "Tag this run so you can separate repeatability work, warmup studies, suspect units, and known-good samples later.");
        _toolTip.SetToolTip(_textBoxAmbientTempC, "Optional for now. Enter ambient temperature in C manually until a fixture sensor is added.");
        _toolTip.SetToolTip(_textBoxAmbientHumidityPct, "Optional for now. Enter ambient relative humidity in percent manually until a fixture sensor is added.");
        _toolTip.SetToolTip(_checkBoxAutoReadSht45, "When enabled in Live ADC mode, read ambient temperature and humidity automatically from an SHT45 at I2C address 0x44 before the run starts.");

        groupBoxInput.Controls.Add(_labelRunTag);
        groupBoxInput.Controls.Add(_comboBoxRunTag);
        groupBoxInput.Controls.Add(_labelAmbientTempC);
        groupBoxInput.Controls.Add(_textBoxAmbientTempC);
        groupBoxInput.Controls.Add(_labelAmbientHumidityPct);
        groupBoxInput.Controls.Add(_textBoxAmbientHumidityPct);
        groupBoxInput.Controls.Add(_checkBoxAutoReadSht45);

        _labelTrendInfo = new Label
        {
            Dock = DockStyle.Top,
            Height = 20,
            Text = "Trend view will populate after you save one or more runs.",
            Padding = new Padding(0, 0, 0, 2)
        };

        _panelTrend = new DoubleBufferedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0)
        };
        _panelTrend.Paint += panelTrend_Paint;

        Panel trendContainer = new()
        {
            Name = "panelTrendContainer",
            Dock = DockStyle.Top,
            Height = 145,
            Padding = new Padding(8, 18, 8, 8)
        };

        trendContainer.Controls.Add(_panelTrend);
        trendContainer.Controls.Add(_labelTrendInfo);
        groupBoxRuns.Controls.Add(trendContainer);
        trendContainer.BringToFront();
    }


    private void ApplyLayoutFixes()
    {
        SuspendLayout();

        try
        {
            const int targetInputHeight = 195;
            int delta = targetInputHeight - groupBoxInput.Height;

            if (delta > 0)
            {
                groupBoxInput.Height = targetInputHeight;

                groupBoxSummary.Top += delta;

                groupBoxSensors.Top += delta;
                groupBoxRuns.Top += delta;
                groupBoxSamples.Top += delta;

                groupBoxSensors.Height = Math.Max(180, groupBoxSensors.Height - delta);
                groupBoxSamples.Height = Math.Max(180, groupBoxSamples.Height - delta);
            }

            if (_labelRunTag is not null)
                _labelRunTag.Location = new Point(19, 96);

            if (_comboBoxRunTag is not null)
            {
                _comboBoxRunTag.Location = new Point(19, 116);
                _comboBoxRunTag.Size = new Size(150, 23);
            }

            if (_labelAmbientTempC is not null)
            {
                _labelAmbientTempC.Text = "Temp °C";
                _labelAmbientTempC.Location = new Point(185, 96);
            }

            if (_textBoxAmbientTempC is not null)
            {
                _textBoxAmbientTempC.Location = new Point(185, 116);
                _textBoxAmbientTempC.Size = new Size(78, 23);
            }

            if (_labelAmbientHumidityPct is not null)
            {
                _labelAmbientHumidityPct.Text = "RH %";
                _labelAmbientHumidityPct.Location = new Point(279, 96);
            }

            if (_textBoxAmbientHumidityPct is not null)
            {
                _textBoxAmbientHumidityPct.Location = new Point(279, 116);
                _textBoxAmbientHumidityPct.Size = new Size(78, 23);
            }

            if (_checkBoxAutoReadSht45 is not null)
            {
                _checkBoxAutoReadSht45.Location = new Point(373, 118);
                _checkBoxAutoReadSht45.AutoSize = true;
            }

            buttonExportRunsCsv.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExportSamplesCsv.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRunCharacterization.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRefreshPorts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxComPort.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            int rightMargin = 16;

            buttonExportSamplesCsv.Location = new Point(
                groupBoxInput.ClientSize.Width - buttonExportSamplesCsv.Width - rightMargin,
                116);

            buttonExportRunsCsv.Location = new Point(
                buttonExportSamplesCsv.Left - 10 - buttonExportRunsCsv.Width,
                116);

            buttonRunCharacterization.Location = new Point(
                buttonExportRunsCsv.Left - 16 - buttonRunCharacterization.Width,
                48);

            buttonRefreshPorts.Location = new Point(
                buttonRunCharacterization.Left - 12 - buttonRefreshPorts.Width,
                49);

            comboBoxComPort.Location = new Point(
                buttonRefreshPorts.Left - 12 - comboBoxComPort.Width,
                51);

            labelComPort.Location = new Point(comboBoxComPort.Left, 33);
            checkBoxUseLiveAdc.Location = new Point(comboBoxComPort.Left, 84);
            labelComPortHint.Location = new Point(comboBoxComPort.Left, 106);

            int textRightLimit = buttonExportRunsCsv.Left - 18;

            checkBoxUseLiveAdc.Width = Math.Max(180, textRightLimit - checkBoxUseLiveAdc.Left);
            labelComPortHint.Width = Math.Max(180, textRightLimit - labelComPortHint.Left);
        }
        finally
        {
            ResumeLayout();
        }
    }

    private void buttonRefreshPorts_Click(object sender, EventArgs e)
    {
        RefreshPorts();
    }

    private void checkBoxUseLiveAdc_CheckedChanged(object sender, EventArgs e)
    {
        UpdateLiveControlsState();
    }

    private void buttonExportRunsCsv_Click(object sender, EventArgs e)
    {
        List<CharacterizationRunRecord> runs = GetVisibleRuns();
        if (runs.Count == 0)
        {
            MessageBox.Show(this, "There are no characterization runs to export.", "No Runs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SensorRecord? sensor = GetSelectedSensor();
        using SaveFileDialog dialog = new()
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = "csv",
            FileName = BuildRunsExportFileName(sensor)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        WriteRunsCsv(dialog.FileName, runs);
        UpdateStatus($"Exported {runs.Count} run(s) to '{Path.GetFileName(dialog.FileName)}'.");
    }

    private void buttonExportSamplesCsv_Click(object sender, EventArgs e)
    {
        CharacterizationRunRecord? run = GetSelectedRun();
        if (run is null)
        {
            MessageBox.Show(this, "Select a run before exporting raw samples.", "No Run Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        List<CharacterizationSampleRecord> samples = GetVisibleSamples();
        if (samples.Count == 0)
        {
            MessageBox.Show(this, "The selected run does not have any raw samples to export.", "No Samples", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using SaveFileDialog dialog = new()
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = "csv",
            FileName = BuildSamplesExportFileName(run)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        WriteSamplesCsv(dialog.FileName, run, samples);
        UpdateStatus($"Exported {samples.Count} sample(s) to '{Path.GetFileName(dialog.FileName)}'.");
    }

    private void buttonAddSensor_Click(object sender, EventArgs e)
    {
        string sensorId = textBoxSensorId.Text.Trim();
        string notes = textBoxNotes.Text.Trim();

        if (string.IsNullOrWhiteSpace(sensorId))
        {
            MessageBox.Show(this, "Enter a Sensor ID before adding a sensor.", "Missing Sensor ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _database.AddSensor(sensorId, notes);
            RefreshSensors();
            SelectSensorById(sensorId);
            UpdateStatus($"Added sensor '{sensorId}'.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Add Sensor Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Add sensor failed.");
        }
    }

    private async void buttonRunCharacterization_Click(object sender, EventArgs e)
    {
        SensorRecord? sensor = GetSelectedSensor();
        if (sensor is null)
        {
            MessageBox.Show(this, "Select a sensor from the grid before running characterization.", "No Sensor Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (checkBoxUseLiveAdc.Checked && string.IsNullOrWhiteSpace(comboBoxComPort.Text))
        {
            MessageBox.Show(this, "Select the Pico COM port before running a live ADC characterization.", "Missing COM Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool shouldAutoReadSht45 = checkBoxUseLiveAdc.Checked && (_checkBoxAutoReadSht45?.Checked == true);

        double? ambientTempC;
        double? ambientHumidityPct;
        try
        {
            ambientTempC = ParseOptionalDouble(_textBoxAmbientTempC?.Text, "Ambient Temp C");
            ambientHumidityPct = ParseOptionalDouble(_textBoxAmbientHumidityPct?.Text, "Ambient RH %");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Invalid Metadata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string runTag = _comboBoxRunTag?.Text.Trim() ?? string.Empty;

        buttonRunCharacterization.Enabled = false;
        buttonAddSensor.Enabled = false;
        buttonRefreshPorts.Enabled = false;
        checkBoxUseLiveAdc.Enabled = false;
        UseWaitCursor = true;

        try
        {
            int warmupMinutes = Decimal.ToInt32(numericUpDownWarmupMinutes.Value);
            int sampleCount = Decimal.ToInt32(numericUpDownSampleCount.Value);
            int sampleIntervalMs = Decimal.ToInt32(numericUpDownSampleIntervalMs.Value);

            if (shouldAutoReadSht45)
            {
                string portName = comboBoxComPort.Text.Trim();
                try
                {
                    UpdateStatus($"Reading SHT45 ambient data on {portName}...");
                    Sht45Reading ambientReading = await _sht45Service.ReadAmbientAsync(portName);
                    ambientTempC = ambientReading.TemperatureC;
                    ambientHumidityPct = ambientReading.RelativeHumidityPct;

                    if (_textBoxAmbientTempC is not null)
                    {
                        _textBoxAmbientTempC.Text = ambientReading.TemperatureC.ToString("F2", CultureInfo.InvariantCulture);
                    }

                    if (_textBoxAmbientHumidityPct is not null)
                    {
                        _textBoxAmbientHumidityPct.Text = ambientReading.RelativeHumidityPct.ToString("F2", CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception ex)
                {
                    DialogResult fallback = MessageBox.Show(
                        this,
                        $"SHT45 auto-read failed.\r\n\r\n{ex.Message}\r\n\r\nSelect Yes to continue with the manual temperature / humidity fields, or No to cancel the run.",
                        "SHT45 Read Failed",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (fallback != DialogResult.Yes)
                    {
                        UpdateStatus("Characterization canceled after SHT45 read failure.");
                        return;
                    }
                }
            }

            CharacterizationResult result;
            if (checkBoxUseLiveAdc.Checked)
            {
                string portName = comboBoxComPort.Text.Trim();
                UpdateStatus($"Running live ADC characterization for '{sensor.SensorId}' on {portName}...");

                result = await _characterizationService.RunLiveAmbientO2Async(
                    portName,
                    warmupMinutes,
                    sampleCount,
                    sampleIntervalMs);
            }
            else
            {
                UpdateStatus($"Running simulated characterization for '{sensor.SensorId}'...");

                result = await _characterizationService.RunSimulatedAmbientO2Async(
                    warmupMinutes,
                    sampleCount,
                    sampleIntervalMs);
            }

            result.RunTag = runTag;
            result.AmbientTempC = ambientTempC;
            result.AmbientHumidityPct = ambientHumidityPct;
            if (shouldAutoReadSht45 && result.RunMode == "Live ADC")
            {
                result.Notes = string.IsNullOrWhiteSpace(result.Notes)
                    ? "Ambient temperature / humidity auto-read from SHT45 at 0x44 before the ADC run."
                    : result.Notes + " Ambient temperature / humidity auto-read from SHT45 at 0x44 before the ADC run.";
            }

            long runId = _database.SaveRun(sensor.Id, result);
            RefreshRuns(sensor.Id);
            SelectRunById(runId);
            RefreshSamples(runId);
            DisplayResult(result);

            string tagText = string.IsNullOrWhiteSpace(runTag) ? "untagged" : runTag;
            UpdateStatus($"Saved characterization run for '{sensor.SensorId}' ({tagText}).");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Characterization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Characterization failed.");
        }
        finally
        {
            UseWaitCursor = false;
            buttonRunCharacterization.Enabled = true;
            buttonAddSensor.Enabled = true;
            buttonRefreshPorts.Enabled = true;
            checkBoxUseLiveAdc.Enabled = true;
            UpdateLiveControlsState();
        }
    }

    private void dataGridViewSensors_SelectionChanged(object sender, EventArgs e)
    {
        SensorRecord? sensor = GetSelectedSensor();
        if (sensor is not null)
        {
            RefreshRuns(sensor.Id);
        }
        else
        {
            UpdateTrend();
        }
    }

    private void dataGridViewRuns_SelectionChanged(object sender, EventArgs e)
    {
        CharacterizationRunRecord? run = GetSelectedRun();
        if (run is null)
        {
            _sampleBindingSource.DataSource = new BindingList<CharacterizationSampleRecord>(new List<CharacterizationSampleRecord>());
            UpdateTrend();
            return;
        }

        RefreshSamples(run.Id);
        DisplayRunSummary(run);
        UpdateTrend();
    }

    private void panelTrend_Paint(object? sender, PaintEventArgs e)
    {
        if (_panelTrend is null)
        {
            return;
        }

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        List<CharacterizationRunRecord> runs = GetVisibleRuns()
            .OrderBy(r => r.RunUtc)
            .ToList();

        Rectangle area = _panelTrend.ClientRectangle;
        area.Inflate(-12, -10);

        if (area.Width <= 20 || area.Height <= 20)
        {
            return;
        }

        using Pen borderPen = new(Color.Gainsboro);
        g.DrawRectangle(borderPen, area);

        if (runs.Count == 0)
        {
            TextRenderer.DrawText(
                g,
                "No runs yet for the selected sensor.",
                Font,
                area,
                Color.DimGray,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        int leftPad = 52;
        int rightPad = 12;
        int topPad = 12;
        int bottomPad = 28;
        Rectangle plot = new(
            area.Left + leftPad,
            area.Top + topPad,
            Math.Max(10, area.Width - leftPad - rightPad),
            Math.Max(10, area.Height - topPad - bottomPad));

        double minValue = runs.Min(r => r.AverageCount);
        double maxValue = runs.Max(r => r.AverageCount);
        if (Math.Abs(maxValue - minValue) < 0.0001)
        {
            minValue -= 1.0;
            maxValue += 1.0;
        }

        using Pen axisPen = new(Color.Silver);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);

        using Font axisFont = new(Font, FontStyle.Regular);
        TextRenderer.DrawText(g, maxValue.ToString("F1", CultureInfo.InvariantCulture), axisFont,
            new Rectangle(area.Left, plot.Top - 8, leftPad - 6, 20), Color.DimGray,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, minValue.ToString("F1", CultureInfo.InvariantCulture), axisFont,
            new Rectangle(area.Left, plot.Bottom - 10, leftPad - 6, 20), Color.DimGray,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

        List<PointF> points = new(runs.Count);
        for (int i = 0; i < runs.Count; i++)
        {
            double xNorm = runs.Count == 1 ? 0.5 : i / (double)(runs.Count - 1);
            double yNorm = (runs[i].AverageCount - minValue) / (maxValue - minValue);

            float x = (float)(plot.Left + (xNorm * plot.Width));
            float y = (float)(plot.Bottom - (yNorm * plot.Height));
            points.Add(new PointF(x, y));
        }

        using Pen linePen = new(Color.SteelBlue, 2f);
        if (points.Count > 1)
        {
            g.DrawLines(linePen, points.ToArray());
        }

        long selectedRunId = GetSelectedRun()?.Id ?? -1;
        using Brush pointBrush = new SolidBrush(Color.SteelBlue);
        using Brush selectedBrush = new SolidBrush(Color.OrangeRed);
        using Brush textBrush = new SolidBrush(Color.DimGray);

        for (int i = 0; i < points.Count; i++)
        {
            CharacterizationRunRecord run = runs[i];
            PointF point = points[i];
            bool isSelected = run.Id == selectedRunId;
            float radius = isSelected ? 5f : 4f;
            RectangleF marker = new(point.X - radius, point.Y - radius, radius * 2f, radius * 2f);
            g.FillEllipse(isSelected ? selectedBrush : pointBrush, marker);
            g.DrawEllipse(Pens.White, marker);

            string xLabel = run.RunUtc.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture);
            Size labelSize = TextRenderer.MeasureText(xLabel, axisFont);
            g.DrawString(xLabel, axisFont, textBrush, point.X - (labelSize.Width / 2f), plot.Bottom + 4);
        }

        if (GetSelectedRun() is CharacterizationRunRecord selectedRun)
        {
            string selectedText = $"Selected: run {selectedRun.Id} | avg {selectedRun.AverageCount:F1} | tag {(string.IsNullOrWhiteSpace(selectedRun.RunTag) ? "-" : selectedRun.RunTag)}";
            TextRenderer.DrawText(g, selectedText, axisFont,
                new Rectangle(plot.Left + 4, area.Top, plot.Width - 8, 18), Color.Black,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    private void RefreshPorts()
    {
        string previousSelection = comboBoxComPort.Text;
        string[] ports = SerialPort.GetPortNames()
            .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        comboBoxComPort.BeginUpdate();
        try
        {
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(ports.Cast<object>().ToArray());
        }
        finally
        {
            comboBoxComPort.EndUpdate();
        }

        if (!string.IsNullOrWhiteSpace(previousSelection) && ports.Contains(previousSelection, StringComparer.OrdinalIgnoreCase))
        {
            comboBoxComPort.SelectedItem = ports.First(p => string.Equals(p, previousSelection, StringComparison.OrdinalIgnoreCase));
        }
        else if (ports.Length == 1)
        {
            comboBoxComPort.SelectedIndex = 0;
        }
        else if (ports.Length == 0)
        {
            comboBoxComPort.Text = string.Empty;
        }

        labelComPortHint.Text = ports.Length == 0
            ? "No COM ports found."
            : $"{ports.Length} port(s) found.";
    }

    private void UpdateLiveControlsState()
    {
        bool useLive = checkBoxUseLiveAdc.Checked;
        bool autoReadSht45 = useLive && (_checkBoxAutoReadSht45?.Checked == true);

        comboBoxComPort.Enabled = useLive;
        buttonRefreshPorts.Enabled = useLive;
        labelComPort.Enabled = useLive;
        labelComPortHint.Enabled = useLive;

        if (_checkBoxAutoReadSht45 is not null)
        {
            _checkBoxAutoReadSht45.Enabled = useLive;
        }

        if (_labelAmbientTempC is not null)
        {
            _labelAmbientTempC.Enabled = !autoReadSht45;
        }

        if (_textBoxAmbientTempC is not null)
        {
            _textBoxAmbientTempC.Enabled = !autoReadSht45;
        }

        if (_labelAmbientHumidityPct is not null)
        {
            _labelAmbientHumidityPct.Enabled = !autoReadSht45;
        }

        if (_textBoxAmbientHumidityPct is not null)
        {
            _textBoxAmbientHumidityPct.Enabled = !autoReadSht45;
        }
    }

    private void RefreshSensors()
    {
        List<SensorRecord> sensors = _database.GetSensors();
        _sensorBindingSource.DataSource = new BindingList<SensorRecord>(sensors);

        if (sensors.Count > 0)
        {
            dataGridViewSensors.ClearSelection();
            dataGridViewSensors.Rows[0].Selected = true;
            dataGridViewSensors.CurrentCell = dataGridViewSensors.Rows[0].Cells[0];
            RefreshRuns(sensors[0].Id);
        }
        else
        {
            _runBindingSource.DataSource = new BindingList<CharacterizationRunRecord>(new List<CharacterizationRunRecord>());
            _sampleBindingSource.DataSource = new BindingList<CharacterizationSampleRecord>(new List<CharacterizationSampleRecord>());
            ClearSummary();
            UpdateTrend();
        }
    }

    private void RefreshRuns(long sensorDbId)
    {
        List<CharacterizationRunRecord> runs = _database.GetRunsForSensor(sensorDbId);
        _runBindingSource.DataSource = new BindingList<CharacterizationRunRecord>(runs);

        if (runs.Count > 0)
        {
            dataGridViewRuns.ClearSelection();
            dataGridViewRuns.Rows[0].Selected = true;
            dataGridViewRuns.CurrentCell = dataGridViewRuns.Rows[0].Cells[0];
            DisplayRunSummary(runs[0]);
            RefreshSamples(runs[0].Id);
        }
        else
        {
            _sampleBindingSource.DataSource = new BindingList<CharacterizationSampleRecord>(new List<CharacterizationSampleRecord>());
            ClearSummary();
        }

        UpdateTrend();
    }

    private void RefreshSamples(long runId)
    {
        List<CharacterizationSampleRecord> samples = _database.GetSamplesForRun(runId);
        _sampleBindingSource.DataSource = new BindingList<CharacterizationSampleRecord>(samples);
    }

    private void SelectSensorById(string sensorId)
    {
        foreach (DataGridViewRow row in dataGridViewSensors.Rows)
        {
            if (row.DataBoundItem is SensorRecord sensor && string.Equals(sensor.SensorId, sensorId, StringComparison.OrdinalIgnoreCase))
            {
                row.Selected = true;
                dataGridViewSensors.CurrentCell = row.Cells[0];
                break;
            }
        }
    }

    private void SelectRunById(long runId)
    {
        foreach (DataGridViewRow row in dataGridViewRuns.Rows)
        {
            if (row.DataBoundItem is CharacterizationRunRecord run && run.Id == runId)
            {
                row.Selected = true;
                dataGridViewRuns.CurrentCell = row.Cells[0];
                break;
            }
        }

        UpdateTrend();
    }

    private SensorRecord? GetSelectedSensor()
    {
        if (dataGridViewSensors.CurrentRow?.DataBoundItem is SensorRecord sensor)
        {
            return sensor;
        }

        if (dataGridViewSensors.SelectedRows.Count > 0 && dataGridViewSensors.SelectedRows[0].DataBoundItem is SensorRecord selectedSensor)
        {
            return selectedSensor;
        }

        return null;
    }

    private CharacterizationRunRecord? GetSelectedRun()
    {
        if (dataGridViewRuns.CurrentRow?.DataBoundItem is CharacterizationRunRecord run)
        {
            return run;
        }

        if (dataGridViewRuns.SelectedRows.Count > 0 && dataGridViewRuns.SelectedRows[0].DataBoundItem is CharacterizationRunRecord selectedRun)
        {
            return selectedRun;
        }

        return null;
    }

    private void DisplayResult(CharacterizationResult result)
    {
        textBoxAverage.Text = result.AverageCount.ToString("F1", CultureInfo.InvariantCulture);
        textBoxMin.Text = result.MinCount.ToString(CultureInfo.InvariantCulture);
        textBoxMax.Text = result.MaxCount.ToString(CultureInfo.InvariantCulture);
        textBoxSpread.Text = result.Spread.ToString(CultureInfo.InvariantCulture);
        textBoxStdDev.Text = result.StdDev.ToString("F2", CultureInfo.InvariantCulture);
        textBoxRunMode.Text = result.RunMode;
        textBoxPortName.Text = result.PortName;
        textBoxAdcAddress.Text = result.AdcAddress;
        textBoxConfigReadback.Text = result.ConfigReadbackHex;
    }

    private void DisplayRunSummary(CharacterizationRunRecord run)
    {
        textBoxAverage.Text = run.AverageCount.ToString("F1", CultureInfo.InvariantCulture);
        textBoxMin.Text = run.MinCount.ToString(CultureInfo.InvariantCulture);
        textBoxMax.Text = run.MaxCount.ToString(CultureInfo.InvariantCulture);
        textBoxSpread.Text = run.Spread.ToString(CultureInfo.InvariantCulture);
        textBoxStdDev.Text = run.StdDev.ToString("F2", CultureInfo.InvariantCulture);
        textBoxRunMode.Text = run.RunMode;
        textBoxPortName.Text = run.PortName;
        textBoxAdcAddress.Text = run.AdcAddress;
        textBoxConfigReadback.Text = run.ConfigReadbackHex;
    }

    private void ClearSummary()
    {
        textBoxAverage.Text = string.Empty;
        textBoxMin.Text = string.Empty;
        textBoxMax.Text = string.Empty;
        textBoxSpread.Text = string.Empty;
        textBoxStdDev.Text = string.Empty;
        textBoxRunMode.Text = string.Empty;
        textBoxPortName.Text = string.Empty;
        textBoxAdcAddress.Text = string.Empty;
        textBoxConfigReadback.Text = string.Empty;
    }

    private List<CharacterizationRunRecord> GetVisibleRuns()
    {
        if (_runBindingSource.DataSource is BindingList<CharacterizationRunRecord> bindingList)
        {
            return bindingList.ToList();
        }

        return new List<CharacterizationRunRecord>();
    }

    private List<CharacterizationSampleRecord> GetVisibleSamples()
    {
        if (_sampleBindingSource.DataSource is BindingList<CharacterizationSampleRecord> bindingList)
        {
            return bindingList.ToList();
        }

        return new List<CharacterizationSampleRecord>();
    }

    private void UpdateTrend()
    {
        if (_panelTrend is null || _labelTrendInfo is null)
        {
            return;
        }

        SensorRecord? sensor = GetSelectedSensor();
        List<CharacterizationRunRecord> runs = GetVisibleRuns()
            .OrderBy(r => r.RunUtc)
            .ToList();

        if (sensor is null)
        {
            _labelTrendInfo.Text = "Select a sensor to view the average-count trend.";
            _panelTrend.Invalidate();
            return;
        }

        if (runs.Count == 0)
        {
            _labelTrendInfo.Text = $"Sensor {sensor.SensorId}: no saved runs yet.";
            _panelTrend.Invalidate();
            return;
        }

        double minAverage = runs.Min(r => r.AverageCount);
        double maxAverage = runs.Max(r => r.AverageCount);
        CharacterizationRunRecord? selectedRun = GetSelectedRun();
        string selectedTag = string.IsNullOrWhiteSpace(selectedRun?.RunTag) ? "-" : selectedRun!.RunTag;

        _labelTrendInfo.Text = $"Sensor {sensor.SensorId} | {runs.Count} run(s) | Avg range {minAverage:F1} to {maxAverage:F1} | Selected tag {selectedTag}";
        _panelTrend.Invalidate();
    }

    private static string BuildRunsExportFileName(SensorRecord? sensor)
    {
        string sensorId = string.IsNullOrWhiteSpace(sensor?.SensorId) ? "all_sensors" : SanitizeFileNamePart(sensor.SensorId);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        return $"o2_runs_{sensorId}_{timestamp}.csv";
    }

    private static string BuildSamplesExportFileName(CharacterizationRunRecord run)
    {
        string sensorId = string.IsNullOrWhiteSpace(run.SensorId) ? "sensor" : SanitizeFileNamePart(run.SensorId);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        return $"o2_samples_{sensorId}_run{run.Id}_{timestamp}.csv";
    }

    private static void WriteRunsCsv(string filePath, IEnumerable<CharacterizationRunRecord> runs)
    {
        StringBuilder csv = new();
        csv.AppendLine("RunId,SensorDbId,SensorId,RunUtc,WarmupMinutes,SampleCount,SampleIntervalMs,AverageCount,MinCount,MaxCount,Spread,StdDev,RunMode,RunTag,AmbientTempC,AmbientHumidityPct,PortName,AdcAddress,ConfigReadbackHex,Notes");

        foreach (CharacterizationRunRecord run in runs)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(run.Id.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.SensorDbId.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.SensorId),
                EscapeCsv(run.RunUtc.ToString("o", CultureInfo.InvariantCulture)),
                EscapeCsv(run.WarmupMinutes.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.SampleCount.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.SampleIntervalMs.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.AverageCount.ToString("F6", CultureInfo.InvariantCulture)),
                EscapeCsv(run.MinCount.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.MaxCount.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.Spread.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.StdDev.ToString("F6", CultureInfo.InvariantCulture)),
                EscapeCsv(run.RunMode),
                EscapeCsv(run.RunTag),
                EscapeCsv(FormatNullableDouble(run.AmbientTempC)),
                EscapeCsv(FormatNullableDouble(run.AmbientHumidityPct)),
                EscapeCsv(run.PortName),
                EscapeCsv(run.AdcAddress),
                EscapeCsv(run.ConfigReadbackHex),
                EscapeCsv(run.Notes)));
        }

        File.WriteAllText(filePath, csv.ToString(), new UTF8Encoding(true));
    }

    private static void WriteSamplesCsv(string filePath, CharacterizationRunRecord run, IEnumerable<CharacterizationSampleRecord> samples)
    {
        StringBuilder csv = new();
        csv.AppendLine("SampleId,RunId,SensorId,RunUtc,RunTag,AmbientTempC,AmbientHumidityPct,SampleIndex,RawCount,TimestampUtc");

        foreach (CharacterizationSampleRecord sample in samples)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(sample.Id.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(sample.RunId.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.SensorId),
                EscapeCsv(run.RunUtc.ToString("o", CultureInfo.InvariantCulture)),
                EscapeCsv(run.RunTag),
                EscapeCsv(FormatNullableDouble(run.AmbientTempC)),
                EscapeCsv(FormatNullableDouble(run.AmbientHumidityPct)),
                EscapeCsv(sample.SampleIndex.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(sample.RawCount.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(sample.TimestampUtc.ToString("o", CultureInfo.InvariantCulture))));
        }

        File.WriteAllText(filePath, csv.ToString(), new UTF8Encoding(true));
    }

    private static double? ParseOptionalDouble(string? text, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (!double.TryParse(text.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value) &&
            !double.TryParse(text.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value))
        {
            throw new InvalidOperationException($"{fieldName} must be blank or a valid number.");
        }

        return value;
    }

    private static string FormatNullableDouble(double? value)
    {
        return value.HasValue ? value.Value.ToString("F2", CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string EscapeCsv(string? value)
    {
        value ??= string.Empty;

        bool mustQuote =
            value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r');

        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
        }

        return mustQuote ? $"\"{value}\"" : value;
    }

    private static string SanitizeFileNamePart(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return string.IsNullOrWhiteSpace(value) ? "export" : value;
    }

    private void UpdateStatus(string message)
    {
        toolStripStatusLabelMain.Text = message;
    }

    private sealed class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
}
