namespace datinate.app {
    partial class ProblemListForm {
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProblemListForm));
            ProblemListView = new com.RADIO.Datinate.App.View.problemList.ProblemListView();
            SuspendLayout();
            // 
            // ProblemListView
            // 
            ProblemListView.Dock = DockStyle.Fill;
            ProblemListView.Location = new Point(0, 0);
            ProblemListView.Margin = new Padding(4, 3, 4, 3);
            ProblemListView.MinimumSize = new Size(347, 456);
            ProblemListView.Name = "ProblemListView";
            ProblemListView.Size = new Size(386, 558);
            ProblemListView.TabIndex = 0;
            // 
            // ProblemListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(386, 558);
            Controls.Add(ProblemListView);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimumSize = new Size(347, 456);
            Name = "ProblemListForm";
            Text = "Problem List";
            FormClosing += OnFormClosing;
            ResumeLayout(false);
        }

        #endregion
        private ProblemListUI unreadableUI;
        private ProblemListUI duplicatesUI;
        public com.RADIO.Datinate.App.View.problemList.ProblemListView ProblemListView;
    }
}