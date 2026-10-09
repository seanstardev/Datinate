using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace datinate.app
{
    partial class ContentPathsHeader
    {
        private IContainer components = null;

        private Panel headerPanel;
        private Label titleLbl;

        private Panel bodyOuterPanel;
        private Panel capPanel;

        private Panel infoPanel;
        private Label infoLbl;

        private Panel columnHeaderPanel;
        private TableLayoutPanel columnLayout;
        private Label moveHdrLbl;
        private Label nameHdrLbl;
        private Label pathHdrLbl;
        private Label browseHdrLbl;
        private Label hideHdrLbl;
        private Label itemsHdrLbl;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            headerPanel = new Panel();
            titleLbl = new Label();
            bodyOuterPanel = new Panel();
            capPanel = new Panel();
            columnHeaderPanel = new Panel();
            columnLayout = new TableLayoutPanel();
            moveHdrLbl = new Label();
            nameHdrLbl = new Label();
            pathHdrLbl = new Label();
            browseHdrLbl = new Label();
            hideHdrLbl = new Label();
            itemsHdrLbl = new Label();
            infoPanel = new Panel();
            infoLbl = new Label();
            headerPanel.SuspendLayout();
            bodyOuterPanel.SuspendLayout();
            capPanel.SuspendLayout();
            columnHeaderPanel.SuspendLayout();
            columnLayout.SuspendLayout();
            infoPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(245, 248, 252);
            headerPanel.Controls.Add(titleLbl);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(0);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(12, 7, 12, 7);
            headerPanel.Size = new Size(600, 34);
            headerPanel.TabIndex = 0;
            // 
            // titleLbl
            // 
            titleLbl.AutoEllipsis = true;
            titleLbl.Dock = DockStyle.Fill;
            titleLbl.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            titleLbl.Location = new Point(12, 7);
            titleLbl.Margin = new Padding(0);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new Size(576, 20);
            titleLbl.TabIndex = 0;
            titleLbl.Text = "Section";
            titleLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // bodyOuterPanel
            // 
            bodyOuterPanel.AutoSize = true;
            bodyOuterPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bodyOuterPanel.BackColor = Color.White;
            bodyOuterPanel.Controls.Add(capPanel);
            bodyOuterPanel.Dock = DockStyle.Top;
            bodyOuterPanel.Location = new Point(0, 34);
            bodyOuterPanel.Margin = new Padding(0);
            bodyOuterPanel.Name = "bodyOuterPanel";
            bodyOuterPanel.Size = new Size(600, 84);
            bodyOuterPanel.TabIndex = 1;
            // 
            // capPanel
            // 
            capPanel.AutoSize = true;
            capPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            capPanel.BackColor = Color.White;
            capPanel.Controls.Add(columnHeaderPanel);
            capPanel.Controls.Add(infoPanel);
            capPanel.Dock = DockStyle.Top;
            capPanel.Location = new Point(0, 0);
            capPanel.Margin = new Padding(0);
            capPanel.Name = "capPanel";
            capPanel.Padding = new Padding(12, 10, 12, 12);
            capPanel.Size = new Size(600, 84);
            capPanel.TabIndex = 0;
            // 
            // columnHeaderPanel
            // 
            columnHeaderPanel.BackColor = Color.FromArgb(248, 250, 253);
            columnHeaderPanel.Controls.Add(columnLayout);
            columnHeaderPanel.Dock = DockStyle.Top;
            columnHeaderPanel.Location = new Point(12, 44);
            columnHeaderPanel.Margin = new Padding(0);
            columnHeaderPanel.Name = "columnHeaderPanel";
            columnHeaderPanel.Size = new Size(576, 28);
            columnHeaderPanel.TabIndex = 1;
            // 
            // columnLayout
            // 
            columnLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            columnLayout.ColumnCount = 6;
            columnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            columnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            columnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            columnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
            columnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            columnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            columnLayout.Controls.Add(moveHdrLbl, 0, 0);
            columnLayout.Controls.Add(nameHdrLbl, 1, 0);
            columnLayout.Controls.Add(pathHdrLbl, 2, 0);
            columnLayout.Controls.Add(browseHdrLbl, 3, 0);
            columnLayout.Controls.Add(hideHdrLbl, 4, 0);
            columnLayout.Controls.Add(itemsHdrLbl, 5, 0);
            columnLayout.Location = new Point(0, 0);
            columnLayout.Margin = new Padding(0);
            columnLayout.Name = "columnLayout";
            columnLayout.RowCount = 1;
            columnLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            columnLayout.Size = new Size(576, 28);
            columnLayout.TabIndex = 0;
            // 
            // moveHdrLbl
            // 
            moveHdrLbl.Dock = DockStyle.Fill;
            moveHdrLbl.Font = new Font("Segoe UI", 9F);
            moveHdrLbl.ForeColor = Color.FromArgb(88, 96, 105);
            moveHdrLbl.Location = new Point(0, 0);
            moveHdrLbl.Margin = new Padding(0);
            moveHdrLbl.Name = "moveHdrLbl";
            moveHdrLbl.Size = new Size(60, 28);
            moveHdrLbl.TabIndex = 0;
            moveHdrLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nameHdrLbl
            // 
            nameHdrLbl.Dock = DockStyle.Fill;
            nameHdrLbl.Font = new Font("Segoe UI", 9F);
            nameHdrLbl.ForeColor = Color.FromArgb(88, 96, 105);
            nameHdrLbl.Location = new Point(60, 0);
            nameHdrLbl.Margin = new Padding(0);
            nameHdrLbl.Name = "nameHdrLbl";
            nameHdrLbl.Size = new Size(200, 28);
            nameHdrLbl.TabIndex = 1;
            nameHdrLbl.Text = "Source";
            nameHdrLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pathHdrLbl
            // 
            pathHdrLbl.Dock = DockStyle.Fill;
            pathHdrLbl.Font = new Font("Segoe UI", 9F);
            pathHdrLbl.ForeColor = Color.FromArgb(88, 96, 105);
            pathHdrLbl.Location = new Point(260, 0);
            pathHdrLbl.Margin = new Padding(0);
            pathHdrLbl.Name = "pathHdrLbl";
            pathHdrLbl.Padding = new Padding(2, 0, 0, 0);
            pathHdrLbl.Size = new Size(132, 28);
            pathHdrLbl.TabIndex = 2;
            pathHdrLbl.Text = "Content Path";
            pathHdrLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // browseHdrLbl
            // 
            browseHdrLbl.Dock = DockStyle.Fill;
            browseHdrLbl.Font = new Font("Segoe UI", 9F);
            browseHdrLbl.ForeColor = Color.FromArgb(88, 96, 105);
            browseHdrLbl.Location = new Point(392, 0);
            browseHdrLbl.Margin = new Padding(0);
            browseHdrLbl.Name = "browseHdrLbl";
            browseHdrLbl.Size = new Size(34, 28);
            browseHdrLbl.TabIndex = 3;
            browseHdrLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // hideHdrLbl
            // 
            hideHdrLbl.Dock = DockStyle.Fill;
            hideHdrLbl.Font = new Font("Segoe UI", 9F);
            hideHdrLbl.ForeColor = Color.FromArgb(88, 96, 105);
            hideHdrLbl.Location = new Point(426, 0);
            hideHdrLbl.Margin = new Padding(0);
            hideHdrLbl.Name = "hideHdrLbl";
            hideHdrLbl.Size = new Size(50, 28);
            hideHdrLbl.TabIndex = 4;
            hideHdrLbl.Text = "Hide";
            hideHdrLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // itemsHdrLbl
            // 
            itemsHdrLbl.Dock = DockStyle.Fill;
            itemsHdrLbl.Font = new Font("Segoe UI", 9F);
            itemsHdrLbl.ForeColor = Color.FromArgb(88, 96, 105);
            itemsHdrLbl.Location = new Point(490, 0);
            itemsHdrLbl.Margin = new Padding(14, 0, 0, 0);
            itemsHdrLbl.Name = "itemsHdrLbl";
            itemsHdrLbl.Size = new Size(86, 28);
            itemsHdrLbl.TabIndex = 5;
            itemsHdrLbl.Text = "Items";
            itemsHdrLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // infoPanel
            // 
            infoPanel.BackColor = Color.FromArgb(250, 252, 255);
            infoPanel.Controls.Add(infoLbl);
            infoPanel.Dock = DockStyle.Top;
            infoPanel.Location = new Point(12, 10);
            infoPanel.Margin = new Padding(0);
            infoPanel.Name = "infoPanel";
            infoPanel.Padding = new Padding(10, 7, 10, 7);
            infoPanel.Size = new Size(576, 34);
            infoPanel.TabIndex = 0;
            // 
            // infoLbl
            // 
            infoLbl.AutoEllipsis = true;
            infoLbl.Dock = DockStyle.Fill;
            infoLbl.Font = new Font("Segoe UI", 9F);
            infoLbl.ForeColor = Color.FromArgb(88, 96, 105);
            infoLbl.Location = new Point(10, 7);
            infoLbl.Margin = new Padding(0);
            infoLbl.Name = "infoLbl";
            infoLbl.Size = new Size(556, 20);
            infoLbl.TabIndex = 0;
            infoLbl.Text = "Info text";
            infoLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // RbContentPathsHeader
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.White;
            Controls.Add(bodyOuterPanel);
            Controls.Add(headerPanel);
            Margin = new Padding(0);
            Name = "RbContentPathsHeader";
            Size = new Size(600, 118);
            headerPanel.ResumeLayout(false);
            bodyOuterPanel.ResumeLayout(false);
            bodyOuterPanel.PerformLayout();
            capPanel.ResumeLayout(false);
            columnHeaderPanel.ResumeLayout(false);
            columnLayout.ResumeLayout(false);
            infoPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
