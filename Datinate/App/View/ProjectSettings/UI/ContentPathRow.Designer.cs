using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace datinate.app
{
    public sealed partial class ContentPathRow
    {
        private IContainer? components;

        private TableLayoutPanel layout;
        private LightBorderTextBox pathTextBox;
        private EllipsisButton browseBtn;
        private CheckBox hideCheckBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            layout = new TableLayoutPanel();
            mediaItemPanel = new Panel();
            mediaItemUI = new MediaItemUI();
            pathPanel = new Panel();
            datLabel = new Label();
            pathTextBox = new LightBorderTextBox();
            browseBtn = new EllipsisButton();
            filesFoldersLabel = new Label();
            hideCheckBox = new CheckBox();
            layout.SuspendLayout();
            mediaItemPanel.SuspendLayout();
            pathPanel.SuspendLayout();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 5;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            layout.Controls.Add(mediaItemPanel, 0, 0);
            layout.Controls.Add(pathPanel, 1, 0);
            layout.Controls.Add(browseBtn, 2, 0);
            layout.Controls.Add(filesFoldersLabel, 3, 0);
            layout.Controls.Add(hideCheckBox, 4, 0);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Margin = new Padding(0);
            layout.Name = "layout";
            layout.RowCount = 1;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layout.Size = new Size(520, 48);
            layout.TabIndex = 0;
            // 
            // mediaItemPanel
            // 
            mediaItemPanel.Controls.Add(mediaItemUI);
            mediaItemPanel.Dock = DockStyle.Fill;
            mediaItemPanel.Location = new Point(0, 0);
            mediaItemPanel.Margin = new Padding(0);
            mediaItemPanel.Name = "mediaItemPanel";
            mediaItemPanel.Size = new Size(240, 48);
            mediaItemPanel.TabIndex = 4;
            // 
            // mediaItemUI
            // 
            mediaItemUI.AutoSize = true;
            mediaItemUI.Dock = DockStyle.Fill;
            mediaItemUI.Location = new Point(0, 0);
            mediaItemUI.MaximumSize = new Size(0, 48);
            mediaItemUI.MinimumSize = new Size(0, 48);
            mediaItemUI.Name = "mediaItemUI";
            mediaItemUI.Size = new Size(240, 48);
            mediaItemUI.TabIndex = 0;
            // 
            // pathPanel
            // 
            pathPanel.Controls.Add(datLabel);
            pathPanel.Controls.Add(pathTextBox);
            pathPanel.Dock = DockStyle.Fill;
            pathPanel.Location = new Point(240, 0);
            pathPanel.Margin = new Padding(0);
            pathPanel.Name = "pathPanel";
            pathPanel.Size = new Size(96, 48);
            pathPanel.TabIndex = 1;
            // 
            // datLabel
            // 
            datLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datLabel.AutoEllipsis = true;
            datLabel.ForeColor = SystemColors.ControlDark;
            datLabel.Location = new Point(8, 33);
            datLabel.Name = "datLabel";
            datLabel.Size = new Size(80, 15);
            datLabel.TabIndex = 2;
            datLabel.Text = "...";
            datLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // pathTextBox
            // 
            pathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pathTextBox.BackColor = Color.White;
            pathTextBox.Location = new Point(8, 5);
            pathTextBox.Margin = new Padding(8, 5, 8, 5);
            pathTextBox.Name = "pathTextBox";
            pathTextBox.Padding = new Padding(6, 0, 6, 0);
            pathTextBox.Size = new Size(80, 27);
            pathTextBox.TabIndex = 1;
            pathTextBox.TabStop = false;
            // 
            // browseBtn
            // 
            browseBtn.Anchor = AnchorStyles.Left;
            browseBtn.Location = new Point(336, 5);
            browseBtn.Margin = new Padding(0, 5, 8, 5);
            browseBtn.Name = "browseBtn";
            browseBtn.Size = new Size(24, 38);
            browseBtn.TabIndex = 2;
            browseBtn.TabStop = false;
            // 
            // filesFoldersLabel
            // 
            filesFoldersLabel.Dock = DockStyle.Fill;
            filesFoldersLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            filesFoldersLabel.ForeColor = SystemColors.ControlDarkDark;
            filesFoldersLabel.Location = new Point(373, 0);
            filesFoldersLabel.Name = "filesFoldersLabel";
            filesFoldersLabel.Size = new Size(94, 48);
            filesFoldersLabel.TabIndex = 5;
            filesFoldersLabel.Text = "Files: 1,234,\r\n\r\nFolders: 18,456";
            filesFoldersLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // hideCheckBox
            // 
            hideCheckBox.Anchor = AnchorStyles.Left;
            hideCheckBox.AutoSize = true;
            hideCheckBox.CheckAlign = ContentAlignment.MiddleCenter;
            hideCheckBox.Location = new Point(470, 17);
            hideCheckBox.Margin = new Padding(0);
            hideCheckBox.Name = "hideCheckBox";
            hideCheckBox.Size = new Size(15, 14);
            hideCheckBox.TabIndex = 3;
            // 
            // RbContentPathRow
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(layout);
            Margin = new Padding(0);
            MinimumSize = new Size(0, 34);
            Name = "RbContentPathRow";
            Size = new Size(520, 48);
            layout.ResumeLayout(false);
            layout.PerformLayout();
            mediaItemPanel.ResumeLayout(false);
            mediaItemPanel.PerformLayout();
            pathPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
        private Panel mediaItemPanel;
        private Label filesFoldersLabel;
        private MediaItemUI mediaItemUI;
        private Panel pathPanel;
        private Label datLabel;
    }
}
