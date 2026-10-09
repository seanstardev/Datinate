namespace datinate.app {
    partial class FilterUI {
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
        private void InitializeComponent()
        {
            filterTextbox = new TextBox();
            goBtn = new Button();
            button1 = new Button();
            SuspendLayout();
            // 
            // filterTextbox
            // 
            filterTextbox.Location = new Point(0, 0);
            filterTextbox.Margin = new Padding(4, 3, 4, 3);
            filterTextbox.Name = "filterTextbox";
            filterTextbox.Size = new Size(185, 23);
            filterTextbox.TabIndex = 1;
            filterTextbox.KeyDown += onKeyDown;
            // 
            // goBtn
            // 
            goBtn.Location = new Point(210, 0);
            goBtn.Margin = new Padding(4, 3, 4, 3);
            goBtn.Name = "goBtn";
            goBtn.Size = new Size(49, 23);
            goBtn.TabIndex = 2;
            goBtn.Text = "GO";
            goBtn.UseVisualStyleBackColor = true;
            goBtn.Click += onGoClick;
            // 
            // button1
            // 
            button1.Location = new Point(187, 0);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(21, 23);
            button1.TabIndex = 3;
            button1.Text = "X";
            button1.UseVisualStyleBackColor = true;
            button1.Click += onClearClick;
            // 
            // FilterUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(button1);
            Controls.Add(goBtn);
            Controls.Add(filterTextbox);
            Margin = new Padding(4, 3, 4, 3);
            Name = "FilterUI";
            Size = new Size(261, 24);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox filterTextbox;
        private System.Windows.Forms.Button goBtn;
        private System.Windows.Forms.Button button1;
    }
}
