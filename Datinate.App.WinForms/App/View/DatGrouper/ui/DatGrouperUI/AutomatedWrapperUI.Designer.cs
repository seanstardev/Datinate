namespace datinate.app
{
    partial class AutomatedWrapperUI
    {
        protected virtual DatGrouperUiBase CreatePrimary()
            => new AutomatedUI();

        protected virtual DatGrouperUiBase CreateSurrogate()
            => new AutomatedUI();


        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
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
            primaryUI = CreatePrimary();
            surrogateUI = CreateSurrogate();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel2 = new Panel();
            titleContainer = new TableLayoutPanel();
            titleUI = new DatGrouperTitleUI();
            panel3 = new Panel();
            panel1 = new Panel();
            tableLayoutPanel1.SuspendLayout();
            panel2.SuspendLayout();
            titleContainer.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // primaryUI
            // 
            primaryUI.AutoValidate = AutoValidate.EnablePreventFocusChange;
            primaryUI.Dock = DockStyle.Fill;
            primaryUI.Location = new Point(0, 0);
            primaryUI.Margin = new Padding(0);
            primaryUI.Name = "primaryUI";
            primaryUI.Size = new Size(896, 518);
            primaryUI.TabIndex = 0;
            // 
            // surrogateUI
            // 
            surrogateUI.Dock = DockStyle.Fill;
            surrogateUI.Location = new Point(0, 0);
            surrogateUI.Margin = new Padding(0);
            surrogateUI.Name = "surrogateUI";
            surrogateUI.Size = new Size(896, 518);
            surrogateUI.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel2, 0, 0);
            tableLayoutPanel1.Controls.Add(panel1, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(896, 554);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.Controls.Add(titleContainer);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(896, 36);
            panel2.TabIndex = 1;
            // 
            // titleContainer
            // 
            titleContainer.ColumnCount = 1;
            titleContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            titleContainer.Controls.Add(titleUI, 0, 0);
            titleContainer.Controls.Add(panel3, 0, 1);
            titleContainer.Dock = DockStyle.Fill;
            titleContainer.Location = new Point(0, 0);
            titleContainer.Margin = new Padding(0);
            titleContainer.Name = "titleContainer";
            titleContainer.RowCount = 2;
            titleContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            titleContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            titleContainer.Size = new Size(896, 36);
            titleContainer.TabIndex = 1;
            // 
            // titleUI
            // 
            titleUI.Anchor = AnchorStyles.None;
            titleUI.BackColor = Color.Transparent;
            titleUI.CornerRadius = 2;
            titleUI.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            titleUI.Location = new Point(362, 0);
            titleUI.Margin = new Padding(0);
            titleUI.Name = "titleUI";
            titleUI.Size = new Size(172, 35);
            titleUI.TabIndex = 0;
            titleUI.TabStop = false;
            titleUI.Text = "Automated";
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ControlDarkDark;
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 35);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(896, 1);
            panel3.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Controls.Add(primaryUI);
            panel1.Controls.Add(surrogateUI);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 36);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(896, 518);
            panel1.TabIndex = 0;
            // 
            // DatGrouperWrapperUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "DatGrouperWrapperUI";
            Size = new Size(896, 554);
            tableLayoutPanel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            titleContainer.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DatGrouperUiBase primaryUI;
        private DatGrouperUiBase surrogateUI;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Panel panel2;
        private DatGrouperTitleUI titleUI;
        private TableLayoutPanel titleContainer;
        private Panel panel3;
    }
}
