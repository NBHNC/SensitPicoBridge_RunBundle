namespace Sensit.App.O2Characterizer;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        labelSensorId = new Label();
        textBoxSensorId = new TextBox();
        labelNotes = new Label();
        textBoxNotes = new TextBox();
        buttonAddSensor = new Button();
        buttonRunCharacterization = new Button();
        dataGridViewSensors = new DataGridView();
        dataGridViewRuns = new DataGridView();
        groupBoxInput = new GroupBox();
        groupBoxSummary = new GroupBox();
        textBoxStdDev = new TextBox();
        labelStdDev = new Label();
        textBoxSpread = new TextBox();
        labelSpread = new Label();
        textBoxMax = new TextBox();
        labelMax = new Label();
        textBoxMin = new TextBox();
        labelMin = new Label();
        textBoxAverage = new TextBox();
        labelAverage = new Label();
        labelDatabasePath = new Label();
        labelDatabasePathValue = new Label();
        statusStripMain = new StatusStrip();
        toolStripStatusLabelMain = new ToolStripStatusLabel();
        labelWarmupMinutes = new Label();
        numericUpDownWarmupMinutes = new NumericUpDown();
        labelSampleCount = new Label();
        numericUpDownSampleCount = new NumericUpDown();
        labelSampleIntervalMs = new Label();
        numericUpDownSampleIntervalMs = new NumericUpDown();
        ((System.ComponentModel.ISupportInitialize)dataGridViewSensors).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dataGridViewRuns).BeginInit();
        groupBoxInput.SuspendLayout();
        groupBoxSummary.SuspendLayout();
        statusStripMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDownWarmupMinutes).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleIntervalMs).BeginInit();
        SuspendLayout();
        // 
        // labelSensorId
        // 
        labelSensorId.AutoSize = true;
        labelSensorId.Location = new Point(18, 31);
        labelSensorId.Name = "labelSensorId";
        labelSensorId.Size = new Size(56, 15);
        labelSensorId.TabIndex = 0;
        labelSensorId.Text = "Sensor ID";
        // 
        // textBoxSensorId
        // 
        textBoxSensorId.Location = new Point(18, 49);
        textBoxSensorId.Name = "textBoxSensorId";
        textBoxSensorId.Size = new Size(176, 23);
        textBoxSensorId.TabIndex = 1;
        // 
        // labelNotes
        // 
        labelNotes.AutoSize = true;
        labelNotes.Location = new Point(210, 31);
        labelNotes.Name = "labelNotes";
        labelNotes.Size = new Size(38, 15);
        labelNotes.TabIndex = 2;
        labelNotes.Text = "Notes";
        // 
        // textBoxNotes
        // 
        textBoxNotes.Location = new Point(210, 49);
        textBoxNotes.Name = "textBoxNotes";
        textBoxNotes.Size = new Size(274, 23);
        textBoxNotes.TabIndex = 3;
        // 
        // buttonAddSensor
        // 
        buttonAddSensor.Location = new Point(500, 47);
        buttonAddSensor.Name = "buttonAddSensor";
        buttonAddSensor.Size = new Size(112, 27);
        buttonAddSensor.TabIndex = 4;
        buttonAddSensor.Text = "Add Sensor";
        buttonAddSensor.UseVisualStyleBackColor = true;
        buttonAddSensor.Click += buttonAddSensor_Click;
        // 
        // buttonRunCharacterization
        // 
        buttonRunCharacterization.Location = new Point(886, 47);
        buttonRunCharacterization.Name = "buttonRunCharacterization";
        buttonRunCharacterization.Size = new Size(172, 27);
        buttonRunCharacterization.TabIndex = 10;
        buttonRunCharacterization.Text = "Run Characterization";
        buttonRunCharacterization.UseVisualStyleBackColor = true;
        buttonRunCharacterization.Click += buttonRunCharacterization_Click;
        // 
        // dataGridViewSensors
        // 
        dataGridViewSensors.AllowUserToAddRows = false;
        dataGridViewSensors.AllowUserToDeleteRows = false;
        dataGridViewSensors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        dataGridViewSensors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewSensors.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewSensors.Location = new Point(12, 146);
        dataGridViewSensors.MultiSelect = false;
        dataGridViewSensors.Name = "dataGridViewSensors";
        dataGridViewSensors.ReadOnly = true;
        dataGridViewSensors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewSensors.Size = new Size(404, 430);
        dataGridViewSensors.TabIndex = 11;
        dataGridViewSensors.SelectionChanged += dataGridViewSensors_SelectionChanged;
        // 
        // dataGridViewRuns
        // 
        dataGridViewRuns.AllowUserToAddRows = false;
        dataGridViewRuns.AllowUserToDeleteRows = false;
        dataGridViewRuns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dataGridViewRuns.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewRuns.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewRuns.Location = new Point(431, 236);
        dataGridViewRuns.Name = "dataGridViewRuns";
        dataGridViewRuns.ReadOnly = true;
        dataGridViewRuns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewRuns.Size = new Size(627, 340);
        dataGridViewRuns.TabIndex = 12;
        // 
        // groupBoxInput
        // 
        groupBoxInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxInput.Controls.Add(labelSensorId);
        groupBoxInput.Controls.Add(textBoxSensorId);
        groupBoxInput.Controls.Add(labelNotes);
        groupBoxInput.Controls.Add(textBoxNotes);
        groupBoxInput.Controls.Add(buttonAddSensor);
        groupBoxInput.Controls.Add(buttonRunCharacterization);
        groupBoxInput.Controls.Add(labelWarmupMinutes);
        groupBoxInput.Controls.Add(numericUpDownWarmupMinutes);
        groupBoxInput.Controls.Add(labelSampleCount);
        groupBoxInput.Controls.Add(numericUpDownSampleCount);
        groupBoxInput.Controls.Add(labelSampleIntervalMs);
        groupBoxInput.Controls.Add(numericUpDownSampleIntervalMs);
        groupBoxInput.Location = new Point(12, 12);
        groupBoxInput.Name = "groupBoxInput";
        groupBoxInput.Size = new Size(1046, 120);
        groupBoxInput.TabIndex = 13;
        groupBoxInput.TabStop = false;
        groupBoxInput.Text = "O2 Characterization Setup";
        // 
        // groupBoxSummary
        // 
        groupBoxSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxSummary.Controls.Add(textBoxStdDev);
        groupBoxSummary.Controls.Add(labelStdDev);
        groupBoxSummary.Controls.Add(textBoxSpread);
        groupBoxSummary.Controls.Add(labelSpread);
        groupBoxSummary.Controls.Add(textBoxMax);
        groupBoxSummary.Controls.Add(labelMax);
        groupBoxSummary.Controls.Add(textBoxMin);
        groupBoxSummary.Controls.Add(labelMin);
        groupBoxSummary.Controls.Add(textBoxAverage);
        groupBoxSummary.Controls.Add(labelAverage);
        groupBoxSummary.Controls.Add(labelDatabasePath);
        groupBoxSummary.Controls.Add(labelDatabasePathValue);
        groupBoxSummary.Location = new Point(431, 146);
        groupBoxSummary.Name = "groupBoxSummary";
        groupBoxSummary.Size = new Size(627, 74);
        groupBoxSummary.TabIndex = 14;
        groupBoxSummary.TabStop = false;
        groupBoxSummary.Text = "Latest Run Summary";
        // 
        // textBoxStdDev
        // 
        textBoxStdDev.Location = new Point(511, 36);
        textBoxStdDev.Name = "textBoxStdDev";
        textBoxStdDev.ReadOnly = true;
        textBoxStdDev.Size = new Size(88, 23);
        textBoxStdDev.TabIndex = 11;
        // 
        // labelStdDev
        // 
        labelStdDev.AutoSize = true;
        labelStdDev.Location = new Point(511, 18);
        labelStdDev.Name = "labelStdDev";
        labelStdDev.Size = new Size(47, 15);
        labelStdDev.TabIndex = 10;
        labelStdDev.Text = "Std Dev";
        // 
        // textBoxSpread
        // 
        textBoxSpread.Location = new Point(417, 36);
        textBoxSpread.Name = "textBoxSpread";
        textBoxSpread.ReadOnly = true;
        textBoxSpread.Size = new Size(76, 23);
        textBoxSpread.TabIndex = 9;
        // 
        // labelSpread
        // 
        labelSpread.AutoSize = true;
        labelSpread.Location = new Point(417, 18);
        labelSpread.Name = "labelSpread";
        labelSpread.Size = new Size(43, 15);
        labelSpread.TabIndex = 8;
        labelSpread.Text = "Spread";
        // 
        // textBoxMax
        // 
        textBoxMax.Location = new Point(330, 36);
        textBoxMax.Name = "textBoxMax";
        textBoxMax.ReadOnly = true;
        textBoxMax.Size = new Size(69, 23);
        textBoxMax.TabIndex = 7;
        // 
        // labelMax
        // 
        labelMax.AutoSize = true;
        labelMax.Location = new Point(330, 18);
        labelMax.Name = "labelMax";
        labelMax.Size = new Size(30, 15);
        labelMax.TabIndex = 6;
        labelMax.Text = "Max";
        // 
        // textBoxMin
        // 
        textBoxMin.Location = new Point(253, 36);
        textBoxMin.Name = "textBoxMin";
        textBoxMin.ReadOnly = true;
        textBoxMin.Size = new Size(59, 23);
        textBoxMin.TabIndex = 5;
        // 
        // labelMin
        // 
        labelMin.AutoSize = true;
        labelMin.Location = new Point(253, 18);
        labelMin.Name = "labelMin";
        labelMin.Size = new Size(28, 15);
        labelMin.TabIndex = 4;
        labelMin.Text = "Min";
        // 
        // textBoxAverage
        // 
        textBoxAverage.Location = new Point(165, 36);
        textBoxAverage.Name = "textBoxAverage";
        textBoxAverage.ReadOnly = true;
        textBoxAverage.Size = new Size(70, 23);
        textBoxAverage.TabIndex = 3;
        // 
        // labelAverage
        // 
        labelAverage.AutoSize = true;
        labelAverage.Location = new Point(165, 18);
        labelAverage.Name = "labelAverage";
        labelAverage.Size = new Size(50, 15);
        labelAverage.TabIndex = 2;
        labelAverage.Text = "Average";
        // 
        // labelDatabasePath
        // 
        labelDatabasePath.AutoSize = true;
        labelDatabasePath.Location = new Point(15, 18);
        labelDatabasePath.Name = "labelDatabasePath";
        labelDatabasePath.Size = new Size(82, 15);
        labelDatabasePath.TabIndex = 0;
        labelDatabasePath.Text = "Database Path";
        // 
        // labelDatabasePathValue
        // 
        labelDatabasePathValue.AutoEllipsis = true;
        labelDatabasePathValue.Location = new Point(15, 39);
        labelDatabasePathValue.Name = "labelDatabasePathValue";
        labelDatabasePathValue.Size = new Size(138, 20);
        labelDatabasePathValue.TabIndex = 1;
        labelDatabasePathValue.Text = "-";
        // 
        // statusStripMain
        // 
        statusStripMain.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelMain });
        statusStripMain.Location = new Point(0, 588);
        statusStripMain.Name = "statusStripMain";
        statusStripMain.Size = new Size(1070, 22);
        statusStripMain.TabIndex = 15;
        statusStripMain.Text = "statusStrip1";
        // 
        // toolStripStatusLabelMain
        // 
        toolStripStatusLabelMain.Name = "toolStripStatusLabelMain";
        toolStripStatusLabelMain.Size = new Size(39, 17);
        toolStripStatusLabelMain.Text = "Ready";
        // 
        // labelWarmupMinutes
        // 
        labelWarmupMinutes.AutoSize = true;
        labelWarmupMinutes.Location = new Point(633, 31);
        labelWarmupMinutes.Name = "labelWarmupMinutes";
        labelWarmupMinutes.Size = new Size(94, 15);
        labelWarmupMinutes.TabIndex = 5;
        labelWarmupMinutes.Text = "Warmup Minutes";
        // 
        // numericUpDownWarmupMinutes
        // 
        numericUpDownWarmupMinutes.Location = new Point(633, 49);
        numericUpDownWarmupMinutes.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        numericUpDownWarmupMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numericUpDownWarmupMinutes.Name = "numericUpDownWarmupMinutes";
        numericUpDownWarmupMinutes.Size = new Size(89, 23);
        numericUpDownWarmupMinutes.TabIndex = 6;
        numericUpDownWarmupMinutes.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // labelSampleCount
        // 
        labelSampleCount.AutoSize = true;
        labelSampleCount.Location = new Point(744, 31);
        labelSampleCount.Name = "labelSampleCount";
        labelSampleCount.Size = new Size(76, 15);
        labelSampleCount.TabIndex = 7;
        labelSampleCount.Text = "Sample Count";
        // 
        // numericUpDownSampleCount
        // 
        numericUpDownSampleCount.Location = new Point(744, 49);
        numericUpDownSampleCount.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
        numericUpDownSampleCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numericUpDownSampleCount.Name = "numericUpDownSampleCount";
        numericUpDownSampleCount.Size = new Size(120, 23);
        numericUpDownSampleCount.TabIndex = 8;
        numericUpDownSampleCount.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // labelSampleIntervalMs
        // 
        labelSampleIntervalMs.AutoSize = true;
        labelSampleIntervalMs.Location = new Point(633, 81);
        labelSampleIntervalMs.Name = "labelSampleIntervalMs";
        labelSampleIntervalMs.Size = new Size(103, 15);
        labelSampleIntervalMs.TabIndex = 9;
        labelSampleIntervalMs.Text = "Sample Interval ms";
        // 
        // numericUpDownSampleIntervalMs
        // 
        numericUpDownSampleIntervalMs.Increment = new decimal(new int[] { 10, 0, 0, 0 });
        numericUpDownSampleIntervalMs.Location = new Point(744, 79);
        numericUpDownSampleIntervalMs.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        numericUpDownSampleIntervalMs.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
        numericUpDownSampleIntervalMs.Name = "numericUpDownSampleIntervalMs";
        numericUpDownSampleIntervalMs.Size = new Size(120, 23);
        numericUpDownSampleIntervalMs.TabIndex = 10;
        numericUpDownSampleIntervalMs.Value = new decimal(new int[] { 100, 0, 0, 0 });
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1070, 610);
        Controls.Add(statusStripMain);
        Controls.Add(groupBoxSummary);
        Controls.Add(groupBoxInput);
        Controls.Add(dataGridViewRuns);
        Controls.Add(dataGridViewSensors);
        MinimumSize = new Size(1086, 649);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "O2 Characterizer";
        Load += Form1_Load;
        ((System.ComponentModel.ISupportInitialize)dataGridViewSensors).EndInit();
        ((System.ComponentModel.ISupportInitialize)dataGridViewRuns).EndInit();
        groupBoxInput.ResumeLayout(false);
        groupBoxInput.PerformLayout();
        groupBoxSummary.ResumeLayout(false);
        groupBoxSummary.PerformLayout();
        statusStripMain.ResumeLayout(false);
        statusStripMain.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDownWarmupMinutes).EndInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleIntervalMs).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label labelSensorId;
    private TextBox textBoxSensorId;
    private Label labelNotes;
    private TextBox textBoxNotes;
    private Button buttonAddSensor;
    private Button buttonRunCharacterization;
    private DataGridView dataGridViewSensors;
    private DataGridView dataGridViewRuns;
    private GroupBox groupBoxInput;
    private GroupBox groupBoxSummary;
    private TextBox textBoxStdDev;
    private Label labelStdDev;
    private TextBox textBoxSpread;
    private Label labelSpread;
    private TextBox textBoxMax;
    private Label labelMax;
    private TextBox textBoxMin;
    private Label labelMin;
    private TextBox textBoxAverage;
    private Label labelAverage;
    private Label labelDatabasePath;
    private Label labelDatabasePathValue;
    private StatusStrip statusStripMain;
    private ToolStripStatusLabel toolStripStatusLabelMain;
    private Label labelWarmupMinutes;
    private NumericUpDown numericUpDownWarmupMinutes;
    private Label labelSampleCount;
    private NumericUpDown numericUpDownSampleCount;
    private Label labelSampleIntervalMs;
    private NumericUpDown numericUpDownSampleIntervalMs;
}
