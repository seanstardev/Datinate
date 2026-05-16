namespace datinate.app {
    partial class CustomListForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomListForm));
            CustomListView = new com.RADIO.Datinate.App.View.customList.CustomListView();
            SuspendLayout();
            // 
            // CustomListView
            // 
            CustomListView.Dock = System.Windows.Forms.DockStyle.Fill;
            CustomListView.Location = new System.Drawing.Point(0, 0);
            CustomListView.Name = "CustomListView";
            CustomListView.Size = new System.Drawing.Size(807, 556);
            CustomListView.TabIndex = 0;
            // 
            // CustomListForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(807, 556);
            Controls.Add(CustomListView);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CustomListForm";
            Text = "Customise DAT";
            FormClosing += OnFormClosing;
            ResumeLayout(false);
        }

        #endregion

        public com.RADIO.Datinate.App.View.customList.CustomListView CustomListView;
    }
}