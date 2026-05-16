namespace datinate.app
{
    partial class MediaItemUI
    {
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
            datChipUI = new DatChipUI();
            descriptionLabel = new Label();
            mediaIconUI = new MediaIconUI();
            ((System.ComponentModel.ISupportInitialize)mediaIconUI).BeginInit();
            SuspendLayout();
            // 
            // datChipUI
            // 
            datChipUI.DatKey = "TOSEC";
            datChipUI.Location = new Point(50, 6);
            datChipUI.Margin = new Padding(0);
            datChipUI.MaximumSize = new Size(40, 20);
            datChipUI.MinimumSize = new Size(40, 20);
            datChipUI.Name = "datChipUI";
            datChipUI.Size = new Size(40, 20);
            datChipUI.TabIndex = 2;
            datChipUI.TabStop = false;
            datChipUI.Text = "datChipui1";
            // 
            // descriptionLabel
            // 
            descriptionLabel.AutoSize = true;
            descriptionLabel.Location = new Point(47, 28);
            descriptionLabel.Name = "descriptionLabel";
            descriptionLabel.Size = new Size(67, 15);
            descriptionLabel.TabIndex = 3;
            descriptionLabel.Text = "Description";
            descriptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // mediaIconUI
            // 
            mediaIconUI.Image = Datinate.Properties.Resources.media_icons_Icon;
            mediaIconUI.ImageKey = null;
            mediaIconUI.Location = new Point(3, 2);
            mediaIconUI.Name = "mediaIconUI";
            mediaIconUI.Size = new Size(44, 44);
            mediaIconUI.SizeMode = PictureBoxSizeMode.Zoom;
            mediaIconUI.TabIndex = 4;
            mediaIconUI.TabStop = false;
            // 
            // MediaItemUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            Controls.Add(datChipUI);
            Controls.Add(descriptionLabel);
            Controls.Add(mediaIconUI);
            Name = "MediaItemUI";
            Size = new Size(336, 49);
            ((System.ComponentModel.ISupportInitialize)mediaIconUI).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DatChipUI datChipUI;
        private Label descriptionLabel;
        private MediaIconUI mediaIconUI;
    }
}
