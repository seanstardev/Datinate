using datinate.app;

namespace com.RADIO.Datinate.App.View.projects {
    partial class ProjectsView {
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
            tabControl = new BorderlessTabControl();
            tabPage1 = new TabPage();
            ProjectLoaderView = new ProjectLoaderView();
            tabPage2 = new TabPage();
            DatGrouperView = new DatGrouperView();
            tabPage3_pathsLoader = new TabPage();
            rbContentPathsView = new DatGrouperProjectSettingsView();
            tabPage4 = new TabPage();
            ExportView = new ExportView();
            tabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3_pathsLoader.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPage1);
            tabControl.Controls.Add(tabPage2);
            tabControl.Controls.Add(tabPage3_pathsLoader);
            tabControl.Controls.Add(tabPage4);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(4, 3, 4, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1164, 788);
            tabControl.TabIndex = 3;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(ProjectLoaderView);
            tabPage1.Location = new Point(0, 20);
            tabPage1.Margin = new Padding(4, 3, 4, 3);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 3, 4, 3);
            tabPage1.Size = new Size(1164, 768);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // ProjectLoaderView
            // 
            ProjectLoaderView.AutoScroll = true;
            ProjectLoaderView.BorderStyle = BorderStyle.FixedSingle;
            ProjectLoaderView.Dock = DockStyle.Fill;
            ProjectLoaderView.Location = new Point(4, 3);
            ProjectLoaderView.Margin = new Padding(0);
            ProjectLoaderView.Name = "ProjectLoaderView";
            ProjectLoaderView.Padding = new Padding(0, 3, 0, 0);
            ProjectLoaderView.Size = new Size(1156, 762);
            ProjectLoaderView.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(DatGrouperView);
            tabPage2.Location = new Point(0, 20);
            tabPage2.Margin = new Padding(4, 3, 4, 3);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4, 3, 4, 3);
            tabPage2.Size = new Size(1164, 768);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // DatGrouperView
            // 
            DatGrouperView.Dock = DockStyle.Fill;
            DatGrouperView.Location = new Point(4, 3);
            DatGrouperView.Margin = new Padding(5, 3, 5, 3);
            DatGrouperView.Name = "DatGrouperView";
            DatGrouperView.Size = new Size(1156, 762);
            DatGrouperView.TabIndex = 1;
            // 
            // tabPage3_pathsLoader
            // 
            tabPage3_pathsLoader.Controls.Add(rbContentPathsView);
            tabPage3_pathsLoader.Location = new Point(0, 20);
            tabPage3_pathsLoader.Name = "tabPage3_pathsLoader";
            tabPage3_pathsLoader.Size = new Size(1164, 768);
            tabPage3_pathsLoader.TabIndex = 2;
            tabPage3_pathsLoader.Text = "tabPage3";
            tabPage3_pathsLoader.UseVisualStyleBackColor = true;
            // 
            // rbContentPathsView
            // 
            rbContentPathsView.BackColor = SystemColors.Control;
            rbContentPathsView.Dock = DockStyle.Fill;
            rbContentPathsView.Location = new Point(0, 0);
            rbContentPathsView.Margin = new Padding(0);
            rbContentPathsView.Name = "rbContentPathsView";
            rbContentPathsView.Size = new Size(1164, 768);
            rbContentPathsView.TabIndex = 0;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(ExportView);
            tabPage4.Location = new Point(0, 20);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(1164, 768);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "tabPage4";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // exportView
            // 
            ExportView.Dock = DockStyle.Fill;
            ExportView.Location = new Point(0, 0);
            ExportView.Name = "exportView";
            ExportView.Size = new Size(1164, 768);
            ExportView.TabIndex = 0;
            // 
            // ProjectsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControl);
            Name = "ProjectsView";
            Size = new Size(1164, 788);
            tabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3_pathsLoader.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        public datinate.app.ProjectLoaderView ProjectLoaderView;
        public datinate.app.DatGrouperView DatGrouperView;
        private TabPage tabPage3_pathsLoader;
        private datinate.app.DatGrouperProjectSettingsView rbContentPathsView;
        private BorderlessTabControl tabControl;
        private TabPage tabPage4;
        public ExportView ExportView;
    }
}
