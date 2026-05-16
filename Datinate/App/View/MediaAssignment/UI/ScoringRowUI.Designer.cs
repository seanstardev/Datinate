namespace datinate.app
{
    partial class ScoringRowUI
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            iconContainer = new FlowLayoutPanel();
            mediaIcon2 = new MediaIconUI();
            mediaIcon1 = new MediaIconUI();
            mediaLabel = new Label();
            confirmationPictureBox = new MediaAcceptanceUI();
            chipOuterPanel = new Panel();
            datChipContainer = new FlowLayoutPanel();
            iconContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mediaIcon2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mediaIcon1).BeginInit();
            chipOuterPanel.SuspendLayout();
            SuspendLayout();
            // 
            // iconContainer
            // 
            iconContainer.Controls.Add(mediaIcon2);
            iconContainer.Controls.Add(mediaIcon1);
            iconContainer.FlowDirection = FlowDirection.RightToLeft;
            iconContainer.Location = new Point(0, 0);
            iconContainer.Margin = new Padding(0);
            iconContainer.Name = "iconContainer";
            iconContainer.Size = new Size(81, 50);
            iconContainer.TabIndex = 0;
            // 
            // mediaIcon2
            // 
            mediaIcon2.Image = Datinate.Properties.Resources.media_icons_Web_Info_description;
            mediaIcon2.ImageKey = null;
            mediaIcon2.Location = new Point(31, 0);
            mediaIcon2.Margin = new Padding(0);
            mediaIcon2.Name = "mediaIcon2";
            mediaIcon2.Size = new Size(50, 50);
            mediaIcon2.SizeMode = PictureBoxSizeMode.Zoom;
            mediaIcon2.TabIndex = 1;
            mediaIcon2.TabStop = false;
            // 
            // mediaIcon1
            // 
            mediaIcon1.Image = Datinate.Properties.Resources.media_icons_Web_info;
            mediaIcon1.ImageKey = null;
            mediaIcon1.Location = new Point(1, 0);
            mediaIcon1.Margin = new Padding(0);
            mediaIcon1.Name = "mediaIcon1";
            mediaIcon1.Size = new Size(30, 50);
            mediaIcon1.SizeMode = PictureBoxSizeMode.Zoom;
            mediaIcon1.TabIndex = 0;
            mediaIcon1.TabStop = false;
            // 
            // mediaLabel
            // 
            mediaLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            mediaLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            mediaLabel.ForeColor = Color.Black;
            mediaLabel.Location = new Point(80, 0);
            mediaLabel.Name = "mediaLabel";
            mediaLabel.Size = new Size(139, 50);
            mediaLabel.TabIndex = 1;
            mediaLabel.Text = "...";
            mediaLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // confirmationPictureBox
            // 
            confirmationPictureBox.BackColor = Color.Transparent;
            confirmationPictureBox.Location = new Point(222, 0);
            confirmationPictureBox.Margin = new Padding(0);
            confirmationPictureBox.MinimumSize = new Size(12, 12);
            confirmationPictureBox.Name = "confirmationPictureBox";
            confirmationPictureBox.Size = new Size(50, 50);
            confirmationPictureBox.TabIndex = 2;
            confirmationPictureBox.TabStop = false;
            // 
            // chipOuterPanel
            // 
            chipOuterPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chipOuterPanel.Controls.Add(datChipContainer);
            chipOuterPanel.Location = new Point(272, 0);
            chipOuterPanel.Margin = new Padding(0);
            chipOuterPanel.Name = "chipOuterPanel";
            chipOuterPanel.Size = new Size(290, 50);
            chipOuterPanel.TabIndex = 4;
            // 
            // datChipContainer
            // 
            datChipContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datChipContainer.Location = new Point(3, 3);
            datChipContainer.Name = "datChipContainer";
            datChipContainer.Size = new Size(284, 44);
            datChipContainer.TabIndex = 18;
            // 
            // ScoringRowUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.White;
            Controls.Add(iconContainer);
            Controls.Add(mediaLabel);
            Controls.Add(confirmationPictureBox);
            Controls.Add(chipOuterPanel);
            Margin = new Padding(0, 0, 0, 4);
            Name = "ScoringRowUI";
            Size = new Size(562, 50);
            iconContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mediaIcon2).EndInit();
            ((System.ComponentModel.ISupportInitialize)mediaIcon1).EndInit();
            chipOuterPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel iconContainer;
        private MediaIconUI mediaIcon2;
        private MediaIconUI mediaIcon1;
        private Label mediaLabel;
        private MediaAcceptanceUI confirmationPictureBox;
        private Panel chipOuterPanel;
        private FlowLayoutPanel datChipContainer;
    }
}