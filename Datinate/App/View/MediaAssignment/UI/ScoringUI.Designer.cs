namespace datinate.app
{
    partial class ScoringUI
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
            rowContainer = new FlowLayoutPanel();
            scoringRowui1 = new ScoringRowUI();
            topRightPB = new PictureBox();
            topLeftPB = new PictureBox();
            bottomRightPB = new PictureBox();
            bottomLeftPB = new PictureBox();
            topFiller = new Panel();
            rowContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)topRightPB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)topLeftPB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bottomRightPB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bottomLeftPB).BeginInit();
            SuspendLayout();
            // 
            // rowContainer
            // 
            rowContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rowContainer.AutoScroll = true;
            rowContainer.BackColor = SystemColors.Control;
            rowContainer.Controls.Add(scoringRowui1);
            rowContainer.FlowDirection = FlowDirection.TopDown;
            rowContainer.Location = new Point(0, 31);
            rowContainer.Name = "rowContainer";
            rowContainer.Size = new Size(765, 352);
            rowContainer.TabIndex = 14;
            rowContainer.WrapContents = false;
            // 
            // scoringRowui1
            // 
            scoringRowui1.AutoSize = true;
            scoringRowui1.BackColor = Color.White;
            scoringRowui1.Location = new Point(0, 0);
            scoringRowui1.Margin = new Padding(0);
            scoringRowui1.Name = "scoringRowui1";
            scoringRowui1.Size = new Size(633, 50);
            scoringRowui1.TabIndex = 0;
            // 
            // topRightPB
            // 
            topRightPB.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            topRightPB.Image = Datinate.Properties.Resources.corner_topRight;
            topRightPB.Location = new Point(745, 5);
            topRightPB.Name = "topRightPB";
            topRightPB.Size = new Size(20, 20);
            topRightPB.SizeMode = PictureBoxSizeMode.Zoom;
            topRightPB.TabIndex = 13;
            topRightPB.TabStop = false;
            // 
            // topLeftPB
            // 
            topLeftPB.Image = Datinate.Properties.Resources.corner_topLeft;
            topLeftPB.Location = new Point(0, 5);
            topLeftPB.Name = "topLeftPB";
            topLeftPB.Size = new Size(20, 20);
            topLeftPB.SizeMode = PictureBoxSizeMode.Zoom;
            topLeftPB.TabIndex = 12;
            topLeftPB.TabStop = false;
            // 
            // bottomRightPB
            // 
            bottomRightPB.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            bottomRightPB.Image = Datinate.Properties.Resources.corner_bottomRight;
            bottomRightPB.Location = new Point(745, 383);
            bottomRightPB.Name = "bottomRightPB";
            bottomRightPB.Size = new Size(20, 20);
            bottomRightPB.SizeMode = PictureBoxSizeMode.Zoom;
            bottomRightPB.TabIndex = 11;
            bottomRightPB.TabStop = false;
            // 
            // bottomLeftPB
            // 
            bottomLeftPB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            bottomLeftPB.Image = Datinate.Properties.Resources.corner_bottomLeft;
            bottomLeftPB.Location = new Point(0, 383);
            bottomLeftPB.Name = "bottomLeftPB";
            bottomLeftPB.Size = new Size(20, 20);
            bottomLeftPB.SizeMode = PictureBoxSizeMode.Zoom;
            bottomLeftPB.TabIndex = 10;
            bottomLeftPB.TabStop = false;
            // 
            // topFiller
            // 
            topFiller.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            topFiller.BackColor = Color.Black;
            topFiller.Location = new Point(0, 0);
            topFiller.Margin = new Padding(0);
            topFiller.Name = "topFiller";
            topFiller.Size = new Size(765, 5);
            topFiller.TabIndex = 15;
            // 
            // ScoringUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(topFiller);
            Controls.Add(rowContainer);
            Controls.Add(topRightPB);
            Controls.Add(topLeftPB);
            Controls.Add(bottomRightPB);
            Controls.Add(bottomLeftPB);
            Margin = new Padding(0);
            Name = "ScoringUI";
            Size = new Size(765, 403);
            rowContainer.ResumeLayout(false);
            rowContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)topRightPB).EndInit();
            ((System.ComponentModel.ISupportInitialize)topLeftPB).EndInit();
            ((System.ComponentModel.ISupportInitialize)bottomRightPB).EndInit();
            ((System.ComponentModel.ISupportInitialize)bottomLeftPB).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel rowContainer;
        private PictureBox topRightPB;
        private PictureBox topLeftPB;
        private PictureBox bottomRightPB;
        private PictureBox bottomLeftPB;
        private ScoringRowUI scoringRowui1;
        private Panel topFiller;
    }
}
