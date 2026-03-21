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
        groupBoxInput = new GroupBox();
        labelComPortHint = new Label();
        buttonRefreshPorts = new Button();
        comboBoxComPort = new ComboBox();
        labelComPort = new Label();
        checkBoxUseLiveAdc = new CheckBox();
        numericUpDownSampleIntervalMs = new NumericUpDown();
        labelSampleIntervalMs = new Label();
        numericUpDownSampleCount = new NumericUpDown();
        labelSampleCount = new Label();
        numericUpDownWarmupMinutes = new NumericUpDown();
        labelWarmupMinutes = new Label();
        buttonRunCharacterization = new Button();
        buttonExportRunsCsv = new Button();
        buttonExportSamplesCsv = new Button();
        buttonAddSensor = new Button();
        textBoxNotes = new TextBox();
        labelNotes = new Label();
        textBoxSensorId = new TextBox();
        labelSensorId = new Label();
        groupBoxSummary = new GroupBox();
        textBoxConfigReadback = new TextBox();
        labelConfigReadback = new Label();
        textBoxAdcAddress = new TextBox();
        labelAdcAddress = new Label();
        textBoxPortName = new TextBox();
        labelPortName = new Label();
        textBoxRunMode = new TextBox();
        labelRunMode = new Label();
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
        labelDatabasePathValue = new Label();
        labelDatabasePath = new Label();
        groupBoxSensors = new GroupBox();
        dataGridViewSensors = new DataGridView();
        groupBoxRuns = new GroupBox();
        dataGridViewRuns = new DataGridView();
        groupBoxSamples = new GroupBox();
        dataGridViewSamples = new DataGridView();
        statusStripMain = new StatusStrip();
        toolStripStatusLabelMain = new ToolStripStatusLabel();
        groupBoxInput.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleIntervalMs).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownWarmupMinutes).BeginInit();
        groupBoxSummary.SuspendLayout();
        groupBoxSensors.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridViewSensors).BeginInit();
        groupBoxRuns.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridViewRuns).BeginInit();
        groupBoxSamples.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridViewSamples).BeginInit();
        statusStripMain.SuspendLayout();
        SuspendLayout();
        // 
        // groupBoxInput
        // 
        groupBoxInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxInput.Controls.Add(labelComPortHint);
        groupBoxInput.Controls.Add(buttonRefreshPorts);
        groupBoxInput.Controls.Add(comboBoxComPort);
        groupBoxInput.Controls.Add(labelComPort);
        groupBoxInput.Controls.Add(checkBoxUseLiveAdc);
        groupBoxInput.Controls.Add(numericUpDownSampleIntervalMs);
        groupBoxInput.Controls.Add(labelSampleIntervalMs);
        groupBoxInput.Controls.Add(numericUpDownSampleCount);
        groupBoxInput.Controls.Add(labelSampleCount);
        groupBoxInput.Controls.Add(numericUpDownWarmupMinutes);
        groupBoxInput.Controls.Add(labelWarmupMinutes);
        groupBoxInput.Controls.Add(buttonExportSamplesCsv);
        groupBoxInput.Controls.Add(buttonExportRunsCsv);
        groupBoxInput.Controls.Add(buttonRunCharacterization);
        groupBoxInput.Controls.Add(buttonAddSensor);
        groupBoxInput.Controls.Add(textBoxNotes);
        groupBoxInput.Controls.Add(labelNotes);
        groupBoxInput.Controls.Add(textBoxSensorId);
        groupBoxInput.Controls.Add(labelSensorId);
        groupBoxInput.Location = new Point(12, 12);
        groupBoxInput.Name = "groupBoxInput";
        groupBoxInput.Size = new Size(1416, 124);
        groupBoxInput.TabIndex = 0;
        groupBoxInput.TabStop = false;
        groupBoxInput.Text = "O2 Characterization Setup";
        // 
        // labelComPortHint
        // 
        labelComPortHint.AutoSize = true;
        labelComPortHint.Location = new Point(833, 104);
        labelComPortHint.Name = "labelComPortHint";
        labelComPortHint.Size = new Size(84, 15);
        labelComPortHint.TabIndex = 16;
        labelComPortHint.Text = "0 port(s) found.";
        // 
        // buttonRefreshPorts
        // 
        buttonRefreshPorts.Location = new Point(1025, 49);
        buttonRefreshPorts.Name = "buttonRefreshPorts";
        buttonRefreshPorts.Size = new Size(80, 27);
        buttonRefreshPorts.TabIndex = 15;
        buttonRefreshPorts.Text = "Refresh";
        buttonRefreshPorts.UseVisualStyleBackColor = true;
        buttonRefreshPorts.Click += buttonRefreshPorts_Click;
        // 
        // comboBoxComPort
        // 
        comboBoxComPort.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxComPort.FormattingEnabled = true;
        comboBoxComPort.Location = new Point(833, 51);
        comboBoxComPort.Name = "comboBoxComPort";
        comboBoxComPort.Size = new Size(176, 23);
        comboBoxComPort.TabIndex = 14;
        // 
        // labelComPort
        // 
        labelComPort.AutoSize = true;
        labelComPort.Location = new Point(833, 33);
        labelComPort.Name = "labelComPort";
        labelComPort.Size = new Size(56, 15);
        labelComPort.TabIndex = 13;
        labelComPort.Text = "Pico COM";
        // 
        // checkBoxUseLiveAdc
        // 
        checkBoxUseLiveAdc.AutoSize = true;
        checkBoxUseLiveAdc.Checked = true;
        checkBoxUseLiveAdc.CheckState = CheckState.Checked;
        checkBoxUseLiveAdc.Location = new Point(833, 82);
        checkBoxUseLiveAdc.Name = "checkBoxUseLiveAdc";
        checkBoxUseLiveAdc.Size = new Size(277, 19);
        checkBoxUseLiveAdc.TabIndex = 12;
        checkBoxUseLiveAdc.Text = "Use live ADC samples through the Pico I2C bridge";
        checkBoxUseLiveAdc.UseVisualStyleBackColor = true;
        checkBoxUseLiveAdc.CheckedChanged += checkBoxUseLiveAdc_CheckedChanged;
        // 
        // numericUpDownSampleIntervalMs
        // 
        numericUpDownSampleIntervalMs.Location = new Point(679, 51);
        numericUpDownSampleIntervalMs.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        numericUpDownSampleIntervalMs.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numericUpDownSampleIntervalMs.Name = "numericUpDownSampleIntervalMs";
        numericUpDownSampleIntervalMs.Size = new Size(120, 23);
        numericUpDownSampleIntervalMs.TabIndex = 11;
        numericUpDownSampleIntervalMs.Value = new decimal(new int[] { 100, 0, 0, 0 });
        // 
        // labelSampleIntervalMs
        // 
        labelSampleIntervalMs.AutoSize = true;
        labelSampleIntervalMs.Location = new Point(679, 33);
        labelSampleIntervalMs.Name = "labelSampleIntervalMs";
        labelSampleIntervalMs.Size = new Size(116, 15);
        labelSampleIntervalMs.TabIndex = 10;
        labelSampleIntervalMs.Text = "Sample Interval (ms)";
        // 
        // numericUpDownSampleCount
        // 
        numericUpDownSampleCount.Location = new Point(548, 51);
        numericUpDownSampleCount.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
        numericUpDownSampleCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numericUpDownSampleCount.Name = "numericUpDownSampleCount";
        numericUpDownSampleCount.Size = new Size(112, 23);
        numericUpDownSampleCount.TabIndex = 9;
        numericUpDownSampleCount.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // labelSampleCount
        // 
        labelSampleCount.AutoSize = true;
        labelSampleCount.Location = new Point(548, 33);
        labelSampleCount.Name = "labelSampleCount";
        labelSampleCount.Size = new Size(76, 15);
        labelSampleCount.TabIndex = 8;
        labelSampleCount.Text = "Sample Count";
        // 
        // numericUpDownWarmupMinutes
        // 
        numericUpDownWarmupMinutes.Location = new Point(419, 51);
        numericUpDownWarmupMinutes.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        numericUpDownWarmupMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numericUpDownWarmupMinutes.Name = "numericUpDownWarmupMinutes";
        numericUpDownWarmupMinutes.Size = new Size(109, 23);
        numericUpDownWarmupMinutes.TabIndex = 7;
        numericUpDownWarmupMinutes.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // labelWarmupMinutes
        // 
        labelWarmupMinutes.AutoSize = true;
        labelWarmupMinutes.Location = new Point(419, 33);
        labelWarmupMinutes.Name = "labelWarmupMinutes";
        labelWarmupMinutes.Size = new Size(94, 15);
        labelWarmupMinutes.TabIndex = 6;
        labelWarmupMinutes.Text = "Warmup Minutes";
        // 
        // buttonRunCharacterization
        // 
        buttonRunCharacterization.Location = new Point(1121, 48);
        buttonRunCharacterization.Name = "buttonRunCharacterization";
        buttonRunCharacterization.Size = new Size(182, 28);
        buttonRunCharacterization.TabIndex = 5;
        buttonRunCharacterization.Text = "Run Characterization";
        buttonRunCharacterization.UseVisualStyleBackColor = true;
        buttonRunCharacterization.Click += buttonRunCharacterization_Click;
        // 
        // buttonExportRunsCsv
        // 
        buttonExportRunsCsv.Location = new Point(1121, 80);
        buttonExportRunsCsv.Name = "buttonExportRunsCsv";
        buttonExportRunsCsv.Size = new Size(110, 28);
        buttonExportRunsCsv.TabIndex = 17;
        buttonExportRunsCsv.Text = "Export Runs CSV";
        buttonExportRunsCsv.UseVisualStyleBackColor = true;
        buttonExportRunsCsv.Click += buttonExportRunsCsv_Click;
        // 
        // buttonExportSamplesCsv
        // 
        buttonExportSamplesCsv.Location = new Point(1237, 80);
        buttonExportSamplesCsv.Name = "buttonExportSamplesCsv";
        buttonExportSamplesCsv.Size = new Size(123, 28);
        buttonExportSamplesCsv.TabIndex = 18;
        buttonExportSamplesCsv.Text = "Export Samples CSV";
        buttonExportSamplesCsv.UseVisualStyleBackColor = true;
        buttonExportSamplesCsv.Click += buttonExportSamplesCsv_Click;
        // 
        // buttonAddSensor
        // 
        buttonAddSensor.Location = new Point(307, 49);
        buttonAddSensor.Name = "buttonAddSensor";
        buttonAddSensor.Size = new Size(92, 27);
        buttonAddSensor.TabIndex = 4;
        buttonAddSensor.Text = "Add Sensor";
        buttonAddSensor.UseVisualStyleBackColor = true;
        buttonAddSensor.Click += buttonAddSensor_Click;
        // 
        // textBoxNotes
        // 
        textBoxNotes.Location = new Point(143, 51);
        textBoxNotes.Name = "textBoxNotes";
        textBoxNotes.Size = new Size(148, 23);
        textBoxNotes.TabIndex = 3;
        // 
        // labelNotes
        // 
        labelNotes.AutoSize = true;
        labelNotes.Location = new Point(143, 33);
        labelNotes.Name = "labelNotes";
        labelNotes.Size = new Size(38, 15);
        labelNotes.TabIndex = 2;
        labelNotes.Text = "Notes";
        // 
        // textBoxSensorId
        // 
        textBoxSensorId.Location = new Point(19, 51);
        textBoxSensorId.Name = "textBoxSensorId";
        textBoxSensorId.Size = new Size(108, 23);
        textBoxSensorId.TabIndex = 1;
        // 
        // labelSensorId
        // 
        labelSensorId.AutoSize = true;
        labelSensorId.Location = new Point(19, 33);
        labelSensorId.Name = "labelSensorId";
        labelSensorId.Size = new Size(56, 15);
        labelSensorId.TabIndex = 0;
        labelSensorId.Text = "Sensor ID";
        // 
        // groupBoxSummary
        // 
        groupBoxSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxSummary.Controls.Add(textBoxConfigReadback);
        groupBoxSummary.Controls.Add(labelConfigReadback);
        groupBoxSummary.Controls.Add(textBoxAdcAddress);
        groupBoxSummary.Controls.Add(labelAdcAddress);
        groupBoxSummary.Controls.Add(textBoxPortName);
        groupBoxSummary.Controls.Add(labelPortName);
        groupBoxSummary.Controls.Add(textBoxRunMode);
        groupBoxSummary.Controls.Add(labelRunMode);
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
        groupBoxSummary.Controls.Add(labelDatabasePathValue);
        groupBoxSummary.Controls.Add(labelDatabasePath);
        groupBoxSummary.Location = new Point(12, 142);
        groupBoxSummary.Name = "groupBoxSummary";
        groupBoxSummary.Size = new Size(1416, 86);
        groupBoxSummary.TabIndex = 1;
        groupBoxSummary.TabStop = false;
        groupBoxSummary.Text = "Latest Run Summary";
        // 
        // textBoxConfigReadback
        // 
        textBoxConfigReadback.Location = new Point(1233, 39);
        textBoxConfigReadback.Name = "textBoxConfigReadback";
        textBoxConfigReadback.ReadOnly = true;
        textBoxConfigReadback.Size = new Size(138, 23);
        textBoxConfigReadback.TabIndex = 19;
        // 
        // labelConfigReadback
        // 
        labelConfigReadback.AutoSize = true;
        labelConfigReadback.Location = new Point(1233, 21);
        labelConfigReadback.Name = "labelConfigReadback";
        labelConfigReadback.Size = new Size(95, 15);
        labelConfigReadback.TabIndex = 18;
        labelConfigReadback.Text = "Config Readback";
        // 
        // textBoxAdcAddress
        // 
        textBoxAdcAddress.Location = new Point(1127, 39);
        textBoxAdcAddress.Name = "textBoxAdcAddress";
        textBoxAdcAddress.ReadOnly = true;
        textBoxAdcAddress.Size = new Size(90, 23);
        textBoxAdcAddress.TabIndex = 17;
        // 
        // labelAdcAddress
        // 
        labelAdcAddress.AutoSize = true;
        labelAdcAddress.Location = new Point(1127, 21);
        labelAdcAddress.Name = "labelAdcAddress";
        labelAdcAddress.Size = new Size(69, 15);
        labelAdcAddress.TabIndex = 16;
        labelAdcAddress.Text = "ADC Addr.";
        // 
        // textBoxPortName
        // 
        textBoxPortName.Location = new Point(984, 39);
        textBoxPortName.Name = "textBoxPortName";
        textBoxPortName.ReadOnly = true;
        textBoxPortName.Size = new Size(127, 23);
        textBoxPortName.TabIndex = 15;
        // 
        // labelPortName
        // 
        labelPortName.AutoSize = true;
        labelPortName.Location = new Point(984, 21);
        labelPortName.Name = "labelPortName";
        labelPortName.Size = new Size(53, 15);
        labelPortName.TabIndex = 14;
        labelPortName.Text = "Port / IO";
        // 
        // textBoxRunMode
        // 
        textBoxRunMode.Location = new Point(870, 39);
        textBoxRunMode.Name = "textBoxRunMode";
        textBoxRunMode.ReadOnly = true;
        textBoxRunMode.Size = new Size(98, 23);
        textBoxRunMode.TabIndex = 13;
        // 
        // labelRunMode
        // 
        labelRunMode.AutoSize = true;
        labelRunMode.Location = new Point(870, 21);
        labelRunMode.Name = "labelRunMode";
        labelRunMode.Size = new Size(60, 15);
        labelRunMode.TabIndex = 12;
        labelRunMode.Text = "Run Mode";
        // 
        // textBoxStdDev
        // 
        textBoxStdDev.Location = new Point(774, 39);
        textBoxStdDev.Name = "textBoxStdDev";
        textBoxStdDev.ReadOnly = true;
        textBoxStdDev.Size = new Size(80, 23);
        textBoxStdDev.TabIndex = 11;
        // 
        // labelStdDev
        // 
        labelStdDev.AutoSize = true;
        labelStdDev.Location = new Point(774, 21);
        labelStdDev.Name = "labelStdDev";
        labelStdDev.Size = new Size(47, 15);
        labelStdDev.TabIndex = 10;
        labelStdDev.Text = "Std Dev";
        // 
        // textBoxSpread
        // 
        textBoxSpread.Location = new Point(684, 39);
        textBoxSpread.Name = "textBoxSpread";
        textBoxSpread.ReadOnly = true;
        textBoxSpread.Size = new Size(74, 23);
        textBoxSpread.TabIndex = 9;
        // 
        // labelSpread
        // 
        labelSpread.AutoSize = true;
        labelSpread.Location = new Point(684, 21);
        labelSpread.Name = "labelSpread";
        labelSpread.Size = new Size(43, 15);
        labelSpread.TabIndex = 8;
        labelSpread.Text = "Spread";
        // 
        // textBoxMax
        // 
        textBoxMax.Location = new Point(600, 39);
        textBoxMax.Name = "textBoxMax";
        textBoxMax.ReadOnly = true;
        textBoxMax.Size = new Size(68, 23);
        textBoxMax.TabIndex = 7;
        // 
        // labelMax
        // 
        labelMax.AutoSize = true;
        labelMax.Location = new Point(600, 21);
        labelMax.Name = "labelMax";
        labelMax.Size = new Size(30, 15);
        labelMax.TabIndex = 6;
        labelMax.Text = "Max";
        // 
        // textBoxMin
        // 
        textBoxMin.Location = new Point(521, 39);
        textBoxMin.Name = "textBoxMin";
        textBoxMin.ReadOnly = true;
        textBoxMin.Size = new Size(63, 23);
        textBoxMin.TabIndex = 5;
        // 
        // labelMin
        // 
        labelMin.AutoSize = true;
        labelMin.Location = new Point(521, 21);
        labelMin.Name = "labelMin";
        labelMin.Size = new Size(28, 15);
        labelMin.TabIndex = 4;
        labelMin.Text = "Min";
        // 
        // textBoxAverage
        // 
        textBoxAverage.Location = new Point(429, 39);
        textBoxAverage.Name = "textBoxAverage";
        textBoxAverage.ReadOnly = true;
        textBoxAverage.Size = new Size(76, 23);
        textBoxAverage.TabIndex = 3;
        // 
        // labelAverage
        // 
        labelAverage.AutoSize = true;
        labelAverage.Location = new Point(429, 21);
        labelAverage.Name = "labelAverage";
        labelAverage.Size = new Size(50, 15);
        labelAverage.TabIndex = 2;
        labelAverage.Text = "Average";
        // 
        // labelDatabasePathValue
        // 
        labelDatabasePathValue.AutoEllipsis = true;
        labelDatabasePathValue.Location = new Point(15, 40);
        labelDatabasePathValue.Name = "labelDatabasePathValue";
        labelDatabasePathValue.Size = new Size(397, 20);
        labelDatabasePathValue.TabIndex = 1;
        labelDatabasePathValue.Text = "-";
        // 
        // labelDatabasePath
        // 
        labelDatabasePath.AutoSize = true;
        labelDatabasePath.Location = new Point(15, 21);
        labelDatabasePath.Name = "labelDatabasePath";
        labelDatabasePath.Size = new Size(82, 15);
        labelDatabasePath.TabIndex = 0;
        labelDatabasePath.Text = "Database Path";
        // 
        // groupBoxSensors
        // 
        groupBoxSensors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        groupBoxSensors.Controls.Add(dataGridViewSensors);
        groupBoxSensors.Location = new Point(12, 234);
        groupBoxSensors.Name = "groupBoxSensors";
        groupBoxSensors.Size = new Size(365, 564);
        groupBoxSensors.TabIndex = 2;
        groupBoxSensors.TabStop = false;
        groupBoxSensors.Text = "Sensors";
        // 
        // dataGridViewSensors
        // 
        dataGridViewSensors.AllowUserToAddRows = false;
        dataGridViewSensors.AllowUserToDeleteRows = false;
        dataGridViewSensors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewSensors.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewSensors.Dock = DockStyle.Fill;
        dataGridViewSensors.MultiSelect = false;
        dataGridViewSensors.Name = "dataGridViewSensors";
        dataGridViewSensors.ReadOnly = true;
        dataGridViewSensors.RowHeadersWidth = 51;
        dataGridViewSensors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewSensors.TabIndex = 0;
        dataGridViewSensors.SelectionChanged += dataGridViewSensors_SelectionChanged;
        // 
        // groupBoxRuns
        // 
        groupBoxRuns.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxRuns.Controls.Add(dataGridViewRuns);
        groupBoxRuns.Location = new Point(383, 234);
        groupBoxRuns.Name = "groupBoxRuns";
        groupBoxRuns.Size = new Size(1045, 268);
        groupBoxRuns.TabIndex = 3;
        groupBoxRuns.TabStop = false;
        groupBoxRuns.Text = "Characterization Runs";
        // 
        // dataGridViewRuns
        // 
        dataGridViewRuns.AllowUserToAddRows = false;
        dataGridViewRuns.AllowUserToDeleteRows = false;
        dataGridViewRuns.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewRuns.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewRuns.Dock = DockStyle.Fill;
        dataGridViewRuns.MultiSelect = false;
        dataGridViewRuns.Name = "dataGridViewRuns";
        dataGridViewRuns.ReadOnly = true;
        dataGridViewRuns.RowHeadersWidth = 51;
        dataGridViewRuns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewRuns.TabIndex = 0;
        dataGridViewRuns.SelectionChanged += dataGridViewRuns_SelectionChanged;
        // 
        // groupBoxSamples
        // 
        groupBoxSamples.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxSamples.Controls.Add(dataGridViewSamples);
        groupBoxSamples.Location = new Point(383, 508);
        groupBoxSamples.Name = "groupBoxSamples";
        groupBoxSamples.Size = new Size(1045, 290);
        groupBoxSamples.TabIndex = 4;
        groupBoxSamples.TabStop = false;
        groupBoxSamples.Text = "Raw Samples for Selected Run";
        // 
        // dataGridViewSamples
        // 
        dataGridViewSamples.AllowUserToAddRows = false;
        dataGridViewSamples.AllowUserToDeleteRows = false;
        dataGridViewSamples.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewSamples.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewSamples.Dock = DockStyle.Fill;
        dataGridViewSamples.Name = "dataGridViewSamples";
        dataGridViewSamples.ReadOnly = true;
        dataGridViewSamples.RowHeadersWidth = 51;
        dataGridViewSamples.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewSamples.TabIndex = 0;
        // 
        // statusStripMain
        // 
        statusStripMain.ImageScalingSize = new Size(20, 20);
        statusStripMain.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelMain });
        statusStripMain.Location = new Point(0, 805);
        statusStripMain.Name = "statusStripMain";
        statusStripMain.Size = new Size(1440, 22);
        statusStripMain.TabIndex = 5;
        statusStripMain.Text = "statusStripMain";
        // 
        // toolStripStatusLabelMain
        // 
        toolStripStatusLabelMain.Name = "toolStripStatusLabelMain";
        toolStripStatusLabelMain.Size = new Size(39, 17);
        toolStripStatusLabelMain.Text = "Ready";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1440, 827);
        Controls.Add(statusStripMain);
        Controls.Add(groupBoxSamples);
        Controls.Add(groupBoxRuns);
        Controls.Add(groupBoxSensors);
        Controls.Add(groupBoxSummary);
        Controls.Add(groupBoxInput);
        MinimumSize = new Size(1250, 700);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "O2 Characterizer";
        Load += Form1_Load;
        groupBoxInput.ResumeLayout(false);
        groupBoxInput.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleIntervalMs).EndInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownSampleCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDownWarmupMinutes).EndInit();
        groupBoxSummary.ResumeLayout(false);
        groupBoxSummary.PerformLayout();
        groupBoxSensors.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dataGridViewSensors).EndInit();
        groupBoxRuns.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dataGridViewRuns).EndInit();
        groupBoxSamples.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dataGridViewSamples).EndInit();
        statusStripMain.ResumeLayout(false);
        statusStripMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private GroupBox groupBoxInput;
    private Label labelSensorId;
    private TextBox textBoxSensorId;
    private Label labelNotes;
    private TextBox textBoxNotes;
    private Button buttonAddSensor;
    private Button buttonRunCharacterization;
    private Button buttonExportRunsCsv;
    private Button buttonExportSamplesCsv;
    private NumericUpDown numericUpDownWarmupMinutes;
    private Label labelWarmupMinutes;
    private NumericUpDown numericUpDownSampleCount;
    private Label labelSampleCount;
    private NumericUpDown numericUpDownSampleIntervalMs;
    private Label labelSampleIntervalMs;
    private CheckBox checkBoxUseLiveAdc;
    private ComboBox comboBoxComPort;
    private Label labelComPort;
    private Button buttonRefreshPorts;
    private Label labelComPortHint;
    private GroupBox groupBoxSummary;
    private Label labelDatabasePath;
    private Label labelDatabasePathValue;
    private TextBox textBoxAverage;
    private Label labelAverage;
    private TextBox textBoxMin;
    private Label labelMin;
    private TextBox textBoxMax;
    private Label labelMax;
    private TextBox textBoxSpread;
    private Label labelSpread;
    private TextBox textBoxStdDev;
    private Label labelStdDev;
    private TextBox textBoxRunMode;
    private Label labelRunMode;
    private TextBox textBoxPortName;
    private Label labelPortName;
    private TextBox textBoxAdcAddress;
    private Label labelAdcAddress;
    private TextBox textBoxConfigReadback;
    private Label labelConfigReadback;
    private GroupBox groupBoxSensors;
    private DataGridView dataGridViewSensors;
    private GroupBox groupBoxRuns;
    private DataGridView dataGridViewRuns;
    private GroupBox groupBoxSamples;
    private DataGridView dataGridViewSamples;
    private StatusStrip statusStripMain;
    private ToolStripStatusLabel toolStripStatusLabelMain;
}
