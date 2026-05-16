namespace com.RADIO.Datinate.App.View.projects.datPathsUpdate {
    partial class DatPathsUpdateView {
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
        private void InitializeComponent() {
            saveBtn = new System.Windows.Forms.Button();
            pathsContainer = new System.Windows.Forms.FlowLayoutPanel();
            SuspendLayout();
            // 
            // saveBtn
            // 
            saveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            saveBtn.Location = new System.Drawing.Point(385, 532);
            saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new System.Drawing.Size(88, 27);
            saveBtn.TabIndex = 5;
            saveBtn.Text = "Save";
            saveBtn.UseVisualStyleBackColor = true;
            saveBtn.Click += SaveBtn_Click;
            // 
            // pathsContainer
            // 
            pathsContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pathsContainer.AutoScroll = true;
            pathsContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pathsContainer.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            pathsContainer.Location = new System.Drawing.Point(21, 31);
            pathsContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pathsContainer.Name = "pathsContainer";
            pathsContainer.Size = new System.Drawing.Size(451, 494);
            pathsContainer.TabIndex = 4;
            pathsContainer.WrapContents = false;
            // 
            // DatPathsUpdateView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(saveBtn);
            Controls.Add(pathsContainer);
            Name = "DatPathsUpdateView";
            Size = new System.Drawing.Size(495, 589);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.FlowLayoutPanel pathsContainer;
    }
}
