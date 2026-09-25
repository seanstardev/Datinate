namespace datinate.app 
{
    partial class DatGrouperUiBase 
    {
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
        private void InitializeComponent()
        {
            TreeNode treeNode1 = new TreeNode("");
            treeView = new DatGrouperTreeView();
            searchPanel = new Panel();
            searchUI = new DatGrouperSearchUI();
            MainContainer = new Panel();
            searchPanel.SuspendLayout();
            MainContainer.SuspendLayout();
            SuspendLayout();
            // 
            // treeView
            // 
            treeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView.BorderStyle = BorderStyle.None;
            treeView.Location = new Point(-30, 0);
            treeView.Margin = new Padding(0);
            treeView.Name = "treeView";
            treeNode1.ForeColor = SystemColors.Window;
            treeNode1.Name = "";
            treeNode1.Text = "";
            treeView.Nodes.AddRange(new TreeNode[] { treeNode1 });
            treeView.ShowPlusMinus = false;
            treeView.Size = new Size(1066, 463);
            treeView.TabIndex = 0;
            // 
            // searchPanel
            // 
            searchPanel.BackColor = Color.White;
            searchPanel.Controls.Add(searchUI);
            searchPanel.Dock = DockStyle.Bottom;
            searchPanel.Location = new Point(0, 466);
            searchPanel.Margin = new Padding(0);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(1036, 29);
            searchPanel.TabIndex = 19;
            // 
            // searchUI
            // 
            searchUI.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            searchUI.Location = new Point(0, 0);
            searchUI.Margin = new Padding(0);
            searchUI.Name = "searchUI";
            searchUI.Size = new Size(1036, 29);
            searchUI.TabIndex = 28;
            // 
            // MainContainer
            // 
            MainContainer.Controls.Add(searchPanel);
            MainContainer.Controls.Add(treeView);
            MainContainer.Dock = DockStyle.Fill;
            MainContainer.Location = new Point(0, 0);
            MainContainer.Margin = new Padding(0);
            MainContainer.Name = "MainContainer";
            MainContainer.Size = new Size(1036, 495);
            MainContainer.TabIndex = 25;
            // 
            // DatGrouperUiBase
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(MainContainer);
            Margin = new Padding(0);
            Name = "DatGrouperUiBase";
            Size = new Size(1036, 495);
            searchPanel.ResumeLayout(false);
            MainContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        protected DatGrouperTreeView treeView;
        private System.Windows.Forms.PictureBox keyExclude;
        private System.Windows.Forms.PictureBox keyConditional;
        private System.Windows.Forms.PictureBox keyInclude;
        private System.Windows.Forms.GroupBox groupBox2;
        private Panel searchPanel;
        private Panel MainContainer;
        private DatGrouperSearchUI searchUI;
    }
}
