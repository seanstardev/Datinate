namespace datinate.app {
    partial class MainControlsView {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                HandleDisposing();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            showUnit = new CheckBox();
            unitsCombo = new ComboBox();
            containerPanel = new Panel();
            splashPanel = new Panel();
            logoPic = new PictureBox();
            controlsPanel = new Panel();
            controlsLayoutPanel = new TableLayoutPanel();
            compareBtn2 = new DatActionButton();
            customiseBtn2 = new DatActionButton();
            datGrouperBtn = new DatActionButton();
            datManagerBtn = new DatActionButton();
            containerPanel.SuspendLayout();
            splashPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPic).BeginInit();
            controlsPanel.SuspendLayout();
            controlsLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 187F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 111F));
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(933, 60);
            tableLayoutPanel1.TabIndex = 11;
            // 
            // showUnit
            // 
            showUnit.AutoSize = true;
            showUnit.CheckAlign = ContentAlignment.MiddleRight;
            showUnit.Checked = true;
            showUnit.CheckState = CheckState.Checked;
            showUnit.Location = new Point(4, 5);
            showUnit.Margin = new Padding(4, 3, 4, 3);
            showUnit.Name = "showUnit";
            showUnit.Size = new Size(126, 19);
            showUnit.TabIndex = 1;
            showUnit.Text = "Show Units in Cells";
            showUnit.UseVisualStyleBackColor = true;
            showUnit.CheckedChanged += onShowUnitChange;
            // 
            // unitsCombo
            // 
            unitsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            unitsCombo.FormattingEnabled = true;
            unitsCombo.Items.AddRange(new object[] { "Bytes", "Kilobytes (KB)", "Megabytes (MB)", "Gigabytes (GB)", "Terabytes (TB)" });
            unitsCombo.Location = new Point(138, 3);
            unitsCombo.Margin = new Padding(4, 3, 4, 3);
            unitsCombo.Name = "unitsCombo";
            unitsCombo.Size = new Size(126, 23);
            unitsCombo.TabIndex = 0;
            unitsCombo.SelectedIndexChanged += onUnitChange;
            // 
            // containerPanel
            // 
            containerPanel.AutoSize = true;
            containerPanel.Controls.Add(controlsPanel);
            containerPanel.Controls.Add(splashPanel);
            containerPanel.Dock = DockStyle.Fill;
            containerPanel.Location = new Point(0, 0);
            containerPanel.Name = "containerPanel";
            containerPanel.Size = new Size(933, 60);
            containerPanel.TabIndex = 14;
            // 
            // splashPanel
            // 
            splashPanel.BackColor = Color.Black;
            splashPanel.Controls.Add(logoPic);
            splashPanel.Dock = DockStyle.Fill;
            splashPanel.Location = new Point(0, 0);
            splashPanel.Margin = new Padding(0);
            splashPanel.Name = "splashPanel";
            splashPanel.Padding = new Padding(0, 32, 0, 6);
            splashPanel.Size = new Size(933, 60);
            splashPanel.TabIndex = 16;
            // 
            // logoPic
            // 
            logoPic.Dock = DockStyle.Fill;
            logoPic.Image = Datinate.Properties.Resources.logo_datinate;
            logoPic.Location = new Point(0, 32);
            logoPic.Name = "logoPic";
            logoPic.Size = new Size(933, 22);
            logoPic.SizeMode = PictureBoxSizeMode.Zoom;
            logoPic.TabIndex = 0;
            logoPic.TabStop = false;
            // 
            // controlsPanel
            // 
            controlsPanel.Controls.Add(controlsLayoutPanel);
            controlsPanel.Controls.Add(showUnit);
            controlsPanel.Controls.Add(unitsCombo);
            controlsPanel.Dock = DockStyle.Fill;
            controlsPanel.Location = new Point(0, 0);
            controlsPanel.Margin = new Padding(0);
            controlsPanel.Name = "controlsPanel";
            controlsPanel.Size = new Size(933, 60);
            controlsPanel.TabIndex = 15;
            // 
            // controlsLayoutPanel
            // 
            controlsLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            controlsLayoutPanel.ColumnCount = 4;
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34.0425529F));
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34.0425529F));
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31.9148922F));
            controlsLayoutPanel.Controls.Add(compareBtn2, 2, 0);
            controlsLayoutPanel.Controls.Add(customiseBtn2, 1, 0);
            controlsLayoutPanel.Controls.Add(datGrouperBtn, 1, 0);
            controlsLayoutPanel.Controls.Add(datManagerBtn, 0, 0);
            controlsLayoutPanel.Location = new Point(449, 13);
            controlsLayoutPanel.Name = "controlsLayoutPanel";
            controlsLayoutPanel.RowCount = 1;
            controlsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            controlsLayoutPanel.Size = new Size(481, 39);
            controlsLayoutPanel.TabIndex = 14;
            // 
            // compareBtn2
            // 
            compareBtn2.BackColor = Color.FromArgb(192, 0, 192);
            compareBtn2.ButtonImage = Datinate.Properties.Resources.datAction_compare;
            compareBtn2.Dock = DockStyle.Fill;
            compareBtn2.Enabled = false;
            compareBtn2.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            compareBtn2.ForeColor = Color.White;
            compareBtn2.Location = new Point(354, 0);
            compareBtn2.Margin = new Padding(12, 0, 0, 0);
            compareBtn2.MaximumFontSize = 16;
            compareBtn2.MinimumSize = new Size(48, 28);
            compareBtn2.Name = "compareBtn2";
            compareBtn2.Padding = new Padding(0, 0, 0, 6);
            compareBtn2.Size = new Size(127, 39);
            compareBtn2.TabIndex = 4;
            compareBtn2.Text = "DAT Compare";
            compareBtn2.Click += compareBtn2_Click;
            // 
            // customiseBtn2
            // 
            customiseBtn2.BackColor = Color.FromArgb(0, 192, 0);
            customiseBtn2.ButtonImage = Datinate.Properties.Resources.datAction_customise;
            customiseBtn2.Dock = DockStyle.Fill;
            customiseBtn2.Enabled = false;
            customiseBtn2.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            customiseBtn2.ForeColor = Color.White;
            customiseBtn2.Location = new Point(207, 0);
            customiseBtn2.Margin = new Padding(12, 0, 0, 0);
            customiseBtn2.MaximumFontSize = 16;
            customiseBtn2.MinimumSize = new Size(48, 28);
            customiseBtn2.Name = "customiseBtn2";
            customiseBtn2.Padding = new Padding(0, 0, 0, 6);
            customiseBtn2.Size = new Size(135, 39);
            customiseBtn2.TabIndex = 3;
            customiseBtn2.Text = "DAT Customiser";
            customiseBtn2.Click += customiseBtn2_Click;
            // 
            // datGrouperBtn
            // 
            datGrouperBtn.BackColor = Color.FromArgb(35, 152, 220);
            datGrouperBtn.ButtonImage = Datinate.Properties.Resources.datAction_grouper;
            datGrouperBtn.Dock = DockStyle.Fill;
            datGrouperBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            datGrouperBtn.ForeColor = Color.White;
            datGrouperBtn.Location = new Point(60, 0);
            datGrouperBtn.Margin = new Padding(12, 0, 0, 0);
            datGrouperBtn.MaximumFontSize = 16;
            datGrouperBtn.MinimumSize = new Size(48, 28);
            datGrouperBtn.Name = "datGrouperBtn";
            datGrouperBtn.Padding = new Padding(0, 0, 0, 6);
            datGrouperBtn.Size = new Size(135, 39);
            datGrouperBtn.TabIndex = 2;
            datGrouperBtn.Text = "DAT Grouper";
            datGrouperBtn.Click += datGrouperBtn_Click;
            // 
            // datManagerBtn
            // 
            datManagerBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datManagerBtn.BackColor = Color.FromArgb(192, 0, 0);
            datManagerBtn.ButtonImage = Datinate.Properties.Resources.datAction_home;
            datManagerBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            datManagerBtn.ForeColor = Color.White;
            datManagerBtn.Location = new Point(0, 0);
            datManagerBtn.Margin = new Padding(0);
            datManagerBtn.MaximumFontSize = 16;
            datManagerBtn.MinimumSize = new Size(48, 28);
            datManagerBtn.Name = "datManagerBtn";
            datManagerBtn.Padding = new Padding(0, 0, 0, 6);
            datManagerBtn.Size = new Size(48, 39);
            datManagerBtn.TabIndex = 15;
            datManagerBtn.Text = "";
            datManagerBtn.Click += datManagerBtn_Click;
            // 
            // MainControlsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(containerPanel);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainControlsView";
            Size = new Size(933, 60);
            containerPanel.ResumeLayout(false);
            splashPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)logoPic).EndInit();
            controlsPanel.ResumeLayout(false);
            controlsPanel.PerformLayout();
            controlsLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox showUnit;
        private System.Windows.Forms.ComboBox unitsCombo;
        private Panel containerPanel;
        private TableLayoutPanel controlsLayoutPanel;
        private DatActionButton datGrouperBtn;
        private DatActionButton customiseBtn2;
        private DatActionButton compareBtn2;
        private Panel controlsPanel;
        private Panel splashPanel;
        private PictureBox logoPic;
        private DatActionButton datManagerBtn;
    }
}
