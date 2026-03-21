using System.ComponentModel;

namespace Sensit.App.O2Characterizer;

public partial class Form1 : Form
{
    private readonly DatabaseService _database;
    private readonly CharacterizationService _characterizationService;
    private readonly BindingSource _sensorBindingSource = new();
    private readonly BindingSource _runBindingSource = new();

    public Form1()
    {
        InitializeComponent();

        _database = new DatabaseService();
        _characterizationService = new CharacterizationService();

        dataGridViewSensors.AutoGenerateColumns = true;
        dataGridViewRuns.AutoGenerateColumns = true;
        dataGridViewSensors.DataSource = _sensorBindingSource;
        dataGridViewRuns.DataSource = _runBindingSource;
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
            RefreshSensors();
            UpdateStatus("Ready.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Startup failed.");
        }
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

        buttonRunCharacterization.Enabled = false;
        buttonAddSensor.Enabled = false;
        UseWaitCursor = true;

        try
        {
            int warmupMinutes = Decimal.ToInt32(numericUpDownWarmupMinutes.Value);
            int sampleCount = Decimal.ToInt32(numericUpDownSampleCount.Value);
            int sampleIntervalMs = Decimal.ToInt32(numericUpDownSampleIntervalMs.Value);

            UpdateStatus($"Running simulated characterization for '{sensor.SensorId}'...");

            CharacterizationResult result = await _characterizationService.RunSimulatedAmbientO2Async(
                warmupMinutes,
                sampleCount,
                sampleIntervalMs);

            _database.SaveRun(sensor.Id, result);
            DisplayResult(result);
            RefreshRuns(sensor.Id);

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

    private void RefreshSensors()
    {
        List<SensorRecord> sensors = _database.GetSensors();
        _sensorBindingSource.DataSource = new BindingList<SensorRecord>(sensors);

        if (sensors.Count > 0)
        {
            dataGridViewSensors.ClearSelection();
            dataGridViewSensors.Rows[0].Selected = true;
            RefreshRuns(sensors[0].Id);
        }
        else
        {
            _runBindingSource.DataSource = new BindingList<CharacterizationRunRecord>(new List<CharacterizationRunRecord>());
        }
    }

    private void RefreshRuns(long sensorDbId)
    {
        List<CharacterizationRunRecord> runs = _database.GetRunsForSensor(sensorDbId);
        _runBindingSource.DataSource = new BindingList<CharacterizationRunRecord>(runs);
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

    private void DisplayResult(CharacterizationResult result)
    {
        textBoxAverage.Text = result.AverageCount.ToString("F1");
        textBoxMin.Text = result.MinCount.ToString();
        textBoxMax.Text = result.MaxCount.ToString();
        textBoxSpread.Text = result.Spread.ToString();
        textBoxStdDev.Text = result.StdDev.ToString("F2");
    }

    private void UpdateStatus(string message)
    {
        toolStripStatusLabelMain.Text = message;
    }
}
