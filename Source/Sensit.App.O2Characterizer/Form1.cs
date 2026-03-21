using System.ComponentModel;
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

    public Form1()
    {
        InitializeComponent();

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

            RefreshPorts();
            RefreshSensors();
            UpdateLiveControlsState();
            UpdateStatus("Ready.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Startup failed.");
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

            long runId = _database.SaveRun(sensor.Id, result);
            RefreshRuns(sensor.Id);
            SelectRunById(runId);
            RefreshSamples(runId);
            DisplayResult(result);

            UpdateStatus($"Saved characterization run for '{sensor.SensorId}'.");
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
    }

    private void dataGridViewRuns_SelectionChanged(object sender, EventArgs e)
    {
        CharacterizationRunRecord? run = GetSelectedRun();
        if (run is null)
        {
            _sampleBindingSource.DataSource = new BindingList<CharacterizationSampleRecord>(new List<CharacterizationSampleRecord>());
            return;
        }

        RefreshSamples(run.Id);
        DisplayRunSummary(run);
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
        comboBoxComPort.Enabled = useLive;
        buttonRefreshPorts.Enabled = useLive;
        labelComPort.Enabled = useLive;
        labelComPortHint.Enabled = useLive;
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
        textBoxAverage.Text = result.AverageCount.ToString("F1");
        textBoxMin.Text = result.MinCount.ToString();
        textBoxMax.Text = result.MaxCount.ToString();
        textBoxSpread.Text = result.Spread.ToString();
        textBoxStdDev.Text = result.StdDev.ToString("F2");
        textBoxRunMode.Text = result.RunMode;
        textBoxPortName.Text = result.PortName;
        textBoxAdcAddress.Text = result.AdcAddress;
        textBoxConfigReadback.Text = result.ConfigReadbackHex;
    }

    private void DisplayRunSummary(CharacterizationRunRecord run)
    {
        textBoxAverage.Text = run.AverageCount.ToString("F1");
        textBoxMin.Text = run.MinCount.ToString();
        textBoxMax.Text = run.MaxCount.ToString();
        textBoxSpread.Text = run.Spread.ToString();
        textBoxStdDev.Text = run.StdDev.ToString("F2");
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
        csv.AppendLine("RunId,SensorDbId,SensorId,RunUtc,WarmupMinutes,SampleCount,SampleIntervalMs,AverageCount,MinCount,MaxCount,Spread,StdDev,RunMode,PortName,AdcAddress,ConfigReadbackHex,Notes");

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
        csv.AppendLine("SampleId,RunId,SensorId,RunUtc,SampleIndex,RawCount,TimestampUtc");

        foreach (CharacterizationSampleRecord sample in samples)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(sample.Id.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(sample.RunId.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(run.SensorId),
                EscapeCsv(run.RunUtc.ToString("o", CultureInfo.InvariantCulture)),
                EscapeCsv(sample.SampleIndex.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(sample.RawCount.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(sample.TimestampUtc.ToString("o", CultureInfo.InvariantCulture))));
        }

        File.WriteAllText(filePath, csv.ToString(), new UTF8Encoding(true));
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
}
