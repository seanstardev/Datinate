
namespace datinate.app {
    partial class DescriptorUI {
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
            descriptorCheck = new System.Windows.Forms.CheckBox();
            excludeCheck = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // descriptorCheck
            // 
            descriptorCheck.AutoSize = true;
            descriptorCheck.Location = new System.Drawing.Point(4, 3);
            descriptorCheck.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            descriptorCheck.Name = "descriptorCheck";
            descriptorCheck.Size = new System.Drawing.Size(31, 19);
            descriptorCheck.TabIndex = 0;
            descriptorCheck.Text = "-";
            descriptorCheck.UseVisualStyleBackColor = true;
            descriptorCheck.CheckedChanged += descriptorCheck_CheckedChanged;
            // 
            // excludeCheck
            // 
            excludeCheck.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            excludeCheck.AutoSize = true;
            excludeCheck.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            excludeCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            excludeCheck.Location = new System.Drawing.Point(207, 3);
            excludeCheck.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            excludeCheck.Name = "excludeCheck";
            excludeCheck.Size = new System.Drawing.Size(67, 16);
            excludeCheck.TabIndex = 1;
            excludeCheck.Text = "Excludes?";
            excludeCheck.UseVisualStyleBackColor = true;
            excludeCheck.CheckedChanged += excludeCheck_CheckedChanged;
            // 
            // DescriptorUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ControlLightLight;
            Controls.Add(excludeCheck);
            Controls.Add(descriptorCheck);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "DescriptorUI";
            Size = new System.Drawing.Size(278, 27);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckBox descriptorCheck;
        private System.Windows.Forms.CheckBox excludeCheck;
    }
}
