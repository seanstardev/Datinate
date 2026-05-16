using System.Drawing;
using System.Windows.Forms;

namespace datinate.app
{
    partial class DatGrouperProjectSettingsView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                HandleDisposing();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            rootLayout = new TableLayoutPanel();
            headerPanel = new Panel();
            panel4 = new Panel();
            titleLbl = new Label();
            bodyPanel = new Panel();
            bodyStackPanel = new Panel();
            sectionsLayout = new TableLayoutPanel();
            softwareCard = new ContentPathsHeader();
            softwarePanel = new AnimatedListPanel();
            auxCard = new ContentPathsHeader();
            auxPanel = new AnimatedListPanel();
            supportCard = new ContentPathsHeader();
            supportPanel = new AnimatedListPanel();
            mediaScoringCard = new ContentPathsHeader();
            mediaDragDropUI = new DragDropUI();
            descriptorsCard = new ContentPathsHeader();
            descriptorsDragDropUI = new DragDropUI();
            bottomBarPanel = new Panel();
            panel3 = new Panel();
            btnLayoutPanel = new FlowLayoutPanel();
            backBtn = new Button();
            saveBtn = new Button();
            batchUpdateBtn = new Button();
            softwareRowsPanel = new Panel();
            auxRowsHostPanel = new Panel();
            supportRowsPanel = new Panel();
            autoSizeContainer = new TableLayoutPanel();
            leftFillerPanel = new Panel();
            panel1 = new Panel();
            rightFillerPanel = new Panel();
            panel2 = new Panel();
            rootLayout.SuspendLayout();
            headerPanel.SuspendLayout();
            bodyPanel.SuspendLayout();
            bodyStackPanel.SuspendLayout();
            sectionsLayout.SuspendLayout();
            bottomBarPanel.SuspendLayout();
            btnLayoutPanel.SuspendLayout();
            autoSizeContainer.SuspendLayout();
            leftFillerPanel.SuspendLayout();
            rightFillerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayout
            // 
            rootLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.Controls.Add(headerPanel, 0, 0);
            rootLayout.Controls.Add(bodyPanel, 0, 1);
            rootLayout.Controls.Add(bottomBarPanel, 0, 2);
            rootLayout.Location = new Point(39, 0);
            rootLayout.Margin = new Padding(0);
            rootLayout.Name = "rootLayout";
            rootLayout.RowCount = 3;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            rootLayout.Size = new Size(800, 666);
            rootLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(panel4);
            headerPanel.Controls.Add(titleLbl);
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(0);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(14, 10, 14, 8);
            headerPanel.Size = new Size(800, 34);
            headerPanel.TabIndex = 0;
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel4.BackColor = SystemColors.ControlDarkDark;
            panel4.Location = new Point(0, 33);
            panel4.Margin = new Padding(0);
            panel4.Name = "panel4";
            panel4.Size = new Size(800, 1);
            panel4.TabIndex = 38;
            // 
            // titleLbl
            // 
            titleLbl.AutoSize = true;
            titleLbl.Dock = DockStyle.Fill;
            titleLbl.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(30, 32, 36);
            titleLbl.Location = new Point(14, 10);
            titleLbl.Margin = new Padding(0);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new Size(208, 20);
            titleLbl.TabIndex = 0;
            titleLbl.Text = "DAT Grouper Project Settings";
            titleLbl.TextAlign = ContentAlignment.BottomLeft;
            // 
            // bodyPanel
            // 
            bodyPanel.AutoScroll = true;
            bodyPanel.BackColor = Color.White;
            bodyPanel.Controls.Add(bodyStackPanel);
            bodyPanel.Dock = DockStyle.Fill;
            bodyPanel.Location = new Point(0, 34);
            bodyPanel.Margin = new Padding(0);
            bodyPanel.Name = "bodyPanel";
            bodyPanel.Padding = new Padding(14, 10, 14, 10);
            bodyPanel.Size = new Size(800, 596);
            bodyPanel.TabIndex = 1;
            // 
            // bodyStackPanel
            // 
            bodyStackPanel.AutoSize = true;
            bodyStackPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bodyStackPanel.Controls.Add(sectionsLayout);
            bodyStackPanel.Dock = DockStyle.Top;
            bodyStackPanel.Location = new Point(14, 10);
            bodyStackPanel.Margin = new Padding(0);
            bodyStackPanel.Name = "bodyStackPanel";
            bodyStackPanel.Size = new Size(755, 806);
            bodyStackPanel.TabIndex = 0;
            // 
            // sectionsLayout
            // 
            sectionsLayout.AutoSize = true;
            sectionsLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            sectionsLayout.ColumnCount = 1;
            sectionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            sectionsLayout.Controls.Add(softwareCard, 0, 0);
            sectionsLayout.Controls.Add(softwarePanel, 0, 1);
            sectionsLayout.Controls.Add(auxCard, 0, 2);
            sectionsLayout.Controls.Add(auxPanel, 0, 3);
            sectionsLayout.Controls.Add(supportCard, 0, 4);
            sectionsLayout.Controls.Add(supportPanel, 0, 5);
            sectionsLayout.Controls.Add(mediaScoringCard, 0, 6);
            sectionsLayout.Controls.Add(mediaDragDropUI, 0, 7);
            sectionsLayout.Controls.Add(descriptorsCard, 0, 8);
            sectionsLayout.Controls.Add(descriptorsDragDropUI, 0, 9);
            sectionsLayout.Dock = DockStyle.Top;
            sectionsLayout.Location = new Point(0, 0);
            sectionsLayout.Margin = new Padding(0);
            sectionsLayout.Name = "sectionsLayout";
            sectionsLayout.RowCount = 10;
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.RowStyles.Add(new RowStyle());
            sectionsLayout.Size = new Size(755, 806);
            sectionsLayout.TabIndex = 0;
            // 
            // softwareCard
            // 
            softwareCard.AutoSize = true;
            softwareCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            softwareCard.BackColor = Color.White;
            softwareCard.Dock = DockStyle.Top;
            softwareCard.Location = new Point(0, 0);
            softwareCard.Margin = new Padding(0);
            softwareCard.Name = "softwareCard";
            softwareCard.ShowHideColumn = false;
            softwareCard.ShowItemsColumn = false;
            softwareCard.Size = new Size(755, 84);
            softwareCard.TabIndex = 0;
            softwareCard.TitleText = "Software Paths";
            softwareCard.Visible = false;
            // 
            // softwarePanel
            // 
            softwarePanel.AutoSize = true;
            softwarePanel.BackColor = Color.White;
            softwarePanel.Dock = DockStyle.Top;
            softwarePanel.Location = new Point(3, 87);
            softwarePanel.Name = "softwarePanel";
            softwarePanel.Size = new Size(749, 0);
            softwarePanel.TabIndex = 2;
            softwarePanel.Visible = false;
            // 
            // auxCard
            // 
            auxCard.AutoSize = true;
            auxCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            auxCard.BackColor = Color.White;
            auxCard.Dock = DockStyle.Top;
            auxCard.InfoText = "Use the arrow buttons or drag and drop to change the order Media Items appear. Set the content paths to preview and assign Media.";
            auxCard.Location = new Point(0, 100);
            auxCard.Margin = new Padding(0, 10, 0, 0);
            auxCard.Name = "auxCard";
            auxCard.Size = new Size(755, 118);
            auxCard.TabIndex = 1;
            auxCard.TitleText = "Media Paths";
            // 
            // auxPanel
            // 
            auxPanel.AutoSize = true;
            auxPanel.BackColor = Color.White;
            auxPanel.Dock = DockStyle.Top;
            auxPanel.Location = new Point(3, 221);
            auxPanel.Name = "auxPanel";
            auxPanel.Size = new Size(749, 0);
            auxPanel.TabIndex = 3;
            // 
            // supportCard
            // 
            supportCard.AutoSize = true;
            supportCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            supportCard.BackColor = Color.White;
            supportCard.ColumnHeadersVisible = false;
            supportCard.Dock = DockStyle.Top;
            supportCard.InfoText = "Support Resource is not assigned for this Project.";
            supportCard.Location = new Point(0, 234);
            supportCard.Margin = new Padding(0, 10, 0, 0);
            supportCard.Name = "supportCard";
            supportCard.Size = new Size(755, 90);
            supportCard.TabIndex = 2;
            supportCard.TitleText = "Support Paths";
            // 
            // supportPanel
            // 
            supportPanel.AutoSize = true;
            supportPanel.BackColor = Color.White;
            supportPanel.Dock = DockStyle.Top;
            supportPanel.Location = new Point(3, 327);
            supportPanel.Name = "supportPanel";
            supportPanel.Size = new Size(749, 0);
            supportPanel.TabIndex = 4;
            // 
            // mediaScoringCard
            // 
            mediaScoringCard.AutoSize = true;
            mediaScoringCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mediaScoringCard.BackColor = Color.White;
            mediaScoringCard.ColumnHeadersVisible = false;
            mediaScoringCard.Dock = DockStyle.Top;
            mediaScoringCard.InfoText = "Drag Media Items into the right panel to include them in this Project's scoring statistics.";
            mediaScoringCard.Location = new Point(0, 340);
            mediaScoringCard.Margin = new Padding(0, 10, 0, 0);
            mediaScoringCard.Name = "mediaScoringCard";
            mediaScoringCard.Size = new Size(755, 90);
            mediaScoringCard.TabIndex = 2;
            mediaScoringCard.TitleText = "Scoring Media";
            // 
            // mediaDragDropUI
            // 
            mediaDragDropUI.AutoSize = true;
            mediaDragDropUI.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mediaDragDropUI.BackColor = Color.White;
            mediaDragDropUI.Dock = DockStyle.Top;
            mediaDragDropUI.Location = new Point(3, 433);
            mediaDragDropUI.Name = "mediaDragDropUI";
            mediaDragDropUI.Size = new Size(749, 132);
            mediaDragDropUI.TabIndex = 1;
            // 
            // descriptorsCard
            // 
            descriptorsCard.AutoSize = true;
            descriptorsCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            descriptorsCard.BackColor = Color.White;
            descriptorsCard.ColumnHeadersVisible = false;
            descriptorsCard.Dock = DockStyle.Top;
            descriptorsCard.InfoText = "Drag Descriptors into the right panel. Families with one or more of these Descriptors will be exempt from scoring.";
            descriptorsCard.Location = new Point(0, 578);
            descriptorsCard.Margin = new Padding(0, 10, 0, 0);
            descriptorsCard.Name = "descriptorsCard";
            descriptorsCard.Size = new Size(755, 90);
            descriptorsCard.TabIndex = 5;
            descriptorsCard.TitleText = "Descriptors";
            // 
            // descriptorsDragDropUI
            // 
            descriptorsDragDropUI.AutoSize = true;
            descriptorsDragDropUI.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            descriptorsDragDropUI.BackColor = Color.White;
            descriptorsDragDropUI.Dock = DockStyle.Top;
            descriptorsDragDropUI.EnableSelectedPanelReorder = false;
            descriptorsDragDropUI.Location = new Point(3, 671);
            descriptorsDragDropUI.Name = "descriptorsDragDropUI";
            descriptorsDragDropUI.Size = new Size(749, 132);
            descriptorsDragDropUI.TabIndex = 6;
            // 
            // bottomBarPanel
            // 
            bottomBarPanel.Controls.Add(panel3);
            bottomBarPanel.Controls.Add(btnLayoutPanel);
            bottomBarPanel.Dock = DockStyle.Bottom;
            bottomBarPanel.Location = new Point(0, 630);
            bottomBarPanel.Margin = new Padding(0);
            bottomBarPanel.Name = "bottomBarPanel";
            bottomBarPanel.Padding = new Padding(14, 10, 14, 10);
            bottomBarPanel.Size = new Size(800, 36);
            bottomBarPanel.TabIndex = 2;
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel3.BackColor = SystemColors.ControlDarkDark;
            panel3.Location = new Point(0, 0);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(800, 1);
            panel3.TabIndex = 37;
            // 
            // btnLayoutPanel
            // 
            btnLayoutPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLayoutPanel.Controls.Add(backBtn);
            btnLayoutPanel.Controls.Add(saveBtn);
            btnLayoutPanel.Controls.Add(batchUpdateBtn);
            btnLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            btnLayoutPanel.Location = new Point(384, 1);
            btnLayoutPanel.Margin = new Padding(0);
            btnLayoutPanel.Name = "btnLayoutPanel";
            btnLayoutPanel.Size = new Size(412, 31);
            btnLayoutPanel.TabIndex = 36;
            // 
            // backBtn
            // 
            backBtn.AutoSize = true;
            backBtn.Cursor = Cursors.Hand;
            backBtn.Location = new Point(312, 3);
            backBtn.Margin = new Padding(0, 3, 0, 0);
            backBtn.Name = "backBtn";
            backBtn.Size = new Size(100, 27);
            backBtn.TabIndex = 1;
            backBtn.Text = "← Exit";
            backBtn.UseVisualStyleBackColor = true;
            backBtn.Click += BackBtn_Click;
            // 
            // saveBtn
            // 
            saveBtn.AutoSize = true;
            saveBtn.Cursor = Cursors.Hand;
            saveBtn.Location = new Point(206, 3);
            saveBtn.Margin = new Padding(0, 3, 6, 0);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new Size(100, 27);
            saveBtn.TabIndex = 0;
            saveBtn.Text = "Save";
            saveBtn.UseVisualStyleBackColor = true;
            saveBtn.Click += SaveBtn_Click;
            // 
            // batchUpdateBtn
            // 
            batchUpdateBtn.AutoSize = true;
            batchUpdateBtn.Cursor = Cursors.Hand;
            batchUpdateBtn.Location = new Point(34, 3);
            batchUpdateBtn.Margin = new Padding(0, 3, 6, 0);
            batchUpdateBtn.Name = "batchUpdateBtn";
            batchUpdateBtn.Size = new Size(166, 27);
            batchUpdateBtn.TabIndex = 2;
            batchUpdateBtn.Text = "Batch Update Content Paths";
            batchUpdateBtn.UseVisualStyleBackColor = true;
            batchUpdateBtn.Click += batchUpdateBtn_Click;
            // 
            // softwareRowsPanel
            // 
            softwareRowsPanel.AutoSize = true;
            softwareRowsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            softwareRowsPanel.Dock = DockStyle.Top;
            softwareRowsPanel.Location = new Point(0, 0);
            softwareRowsPanel.Margin = new Padding(0);
            softwareRowsPanel.Name = "softwareRowsPanel";
            softwareRowsPanel.Size = new Size(200, 0);
            softwareRowsPanel.TabIndex = 0;
            // 
            // auxRowsHostPanel
            // 
            auxRowsHostPanel.AutoSize = true;
            auxRowsHostPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            auxRowsHostPanel.Dock = DockStyle.Top;
            auxRowsHostPanel.Location = new Point(0, 0);
            auxRowsHostPanel.Margin = new Padding(0);
            auxRowsHostPanel.Name = "auxRowsHostPanel";
            auxRowsHostPanel.Size = new Size(200, 0);
            auxRowsHostPanel.TabIndex = 0;
            // 
            // supportRowsPanel
            // 
            supportRowsPanel.AutoSize = true;
            supportRowsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            supportRowsPanel.Dock = DockStyle.Top;
            supportRowsPanel.Location = new Point(0, 0);
            supportRowsPanel.Margin = new Padding(0);
            supportRowsPanel.Name = "supportRowsPanel";
            supportRowsPanel.Size = new Size(200, 0);
            supportRowsPanel.TabIndex = 0;
            // 
            // autoSizeContainer
            // 
            autoSizeContainer.ColumnCount = 3;
            autoSizeContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            autoSizeContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 800F));
            autoSizeContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            autoSizeContainer.Controls.Add(leftFillerPanel, 0, 0);
            autoSizeContainer.Controls.Add(rightFillerPanel, 2, 0);
            autoSizeContainer.Controls.Add(rootLayout, 1, 0);
            autoSizeContainer.Dock = DockStyle.Fill;
            autoSizeContainer.Location = new Point(0, 0);
            autoSizeContainer.Margin = new Padding(0);
            autoSizeContainer.Name = "autoSizeContainer";
            autoSizeContainer.RowCount = 1;
            autoSizeContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            autoSizeContainer.Size = new Size(879, 666);
            autoSizeContainer.TabIndex = 1;
            // 
            // leftFillerPanel
            // 
            leftFillerPanel.BackColor = SystemColors.ControlDarkDark;
            leftFillerPanel.Controls.Add(panel1);
            leftFillerPanel.Dock = DockStyle.Fill;
            leftFillerPanel.Location = new Point(0, 0);
            leftFillerPanel.Margin = new Padding(0);
            leftFillerPanel.Name = "leftFillerPanel";
            leftFillerPanel.Size = new Size(39, 666);
            leftFillerPanel.TabIndex = 2;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = SystemColors.Control;
            panel1.Location = new Point(0, 631);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(39, 35);
            panel1.TabIndex = 1;
            // 
            // rightFillerPanel
            // 
            rightFillerPanel.BackColor = SystemColors.ControlDarkDark;
            rightFillerPanel.Controls.Add(panel2);
            rightFillerPanel.Dock = DockStyle.Fill;
            rightFillerPanel.Location = new Point(839, 0);
            rightFillerPanel.Margin = new Padding(0);
            rightFillerPanel.Name = "rightFillerPanel";
            rightFillerPanel.Size = new Size(40, 666);
            rightFillerPanel.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = SystemColors.Control;
            panel2.Location = new Point(0, 631);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(39, 35);
            panel2.TabIndex = 2;
            // 
            // DatGrouperProjectSettingsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(autoSizeContainer);
            Margin = new Padding(0);
            Name = "DatGrouperProjectSettingsView";
            Size = new Size(879, 666);
            rootLayout.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            bodyPanel.ResumeLayout(false);
            bodyPanel.PerformLayout();
            bodyStackPanel.ResumeLayout(false);
            bodyStackPanel.PerformLayout();
            sectionsLayout.ResumeLayout(false);
            sectionsLayout.PerformLayout();
            bottomBarPanel.ResumeLayout(false);
            btnLayoutPanel.ResumeLayout(false);
            btnLayoutPanel.PerformLayout();
            autoSizeContainer.ResumeLayout(false);
            leftFillerPanel.ResumeLayout(false);
            rightFillerPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel rootLayout;
        private Panel headerPanel;
        private Label titleLbl;
        private Panel bodyPanel;
        private Panel bottomBarPanel;
        private FlowLayoutPanel bottomBarFlow;
        private Button saveBtn;
        private Button backBtn;

        private Panel bodyStackPanel;
        private Panel softwareRowsPanel;
        private Panel auxRowsHostPanel;
        private Panel supportRowsPanel;

        private TableLayoutPanel sectionsLayout;
        private ContentPathsHeader softwareCard;
        private AnimatedListPanel softwarePanel;
        private ContentPathsHeader auxCard;
        private AnimatedListPanel auxPanel;
        private ContentPathsHeader supportCard;
        private AnimatedListPanel supportPanel;
        private ContentPathsHeader mediaScoringCard;
        private DragDropUI mediaDragDropUI;
        private ContentPathsHeader descriptorsCard;
        private DragDropUI descriptorsDragDropUI;
        private Button batchUpdateBtn;
        private FlowLayoutPanel btnLayoutPanel;
        private TableLayoutPanel autoSizeContainer;
        private Panel leftFillerPanel;
        private Panel rightFillerPanel;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
    }
}