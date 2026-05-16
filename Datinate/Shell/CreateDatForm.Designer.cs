namespace datinate.app {
    partial class CreateDatForm {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateDatForm));
            CreateDatView = new com.RADIO.Datinate.App.View.createDat.CreateDatView();
            SuspendLayout();
            // 
            // createDatView1
            // 
            CreateDatView.Dock = System.Windows.Forms.DockStyle.Fill;
            CreateDatView.Location = new System.Drawing.Point(0, 0);
            CreateDatView.Name = "createDatView1";
            CreateDatView.Size = new System.Drawing.Size(372, 615);
            CreateDatView.TabIndex = 0;
            // 
            // CreateDatForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(372, 615);
            Controls.Add(CreateDatView);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "CreateDatForm";
            Text = "Create DAT";
            FormClosing += onFormClosing;
            ResumeLayout(false);
        }

        #endregion

        public com.RADIO.Datinate.App.View.createDat.CreateDatView CreateDatView;
    }
}