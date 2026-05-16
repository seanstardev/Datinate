using datinate.app;

namespace com.RADIO.Datinate {
    partial class MainView {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            mainLayoutPanel = new TableLayoutPanel();
            splitContainer = new SplitContainer();
            tabControl = new BorderlessTabControl();
            landingPage = new TabPage();
            landingView = new LandingView();
            datManagerPage = new TabPage();
            datSummaryView = new DatSummaryView();
            datDetailsView = new DatDetailsView();
            mainControlsView = new MainControlsView();
            mainLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tabControl.SuspendLayout();
            landingPage.SuspendLayout();
            datManagerPage.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            mainLayoutPanel.BackColor = SystemColors.Control;
            mainLayoutPanel.ColumnCount = 1;
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayoutPanel.Controls.Add(splitContainer, 0, 0);
            mainLayoutPanel.Controls.Add(mainControlsView, 0, 1);
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.Location = new Point(0, 0);
            mainLayoutPanel.Margin = new Padding(0);
            mainLayoutPanel.Name = "mainLayoutPanel";
            mainLayoutPanel.RowCount = 2;
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainLayoutPanel.Size = new Size(949, 559);
            mainLayoutPanel.TabIndex = 10;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Margin = new Padding(0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(tabControl);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(datDetailsView);
            splitContainer.Panel2MinSize = 506;
            splitContainer.Size = new Size(949, 499);
            splitContainer.SplitterDistance = 435;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 8;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(landingPage);
            tabControl.Controls.Add(datManagerPage);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.Padding = new Point(0, 0);
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(435, 499);
            tabControl.TabIndex = 5;
            // 
            // landingPage
            // 
            landingPage.Controls.Add(landingView);
            landingPage.Location = new Point(0, 20);
            landingPage.Margin = new Padding(0);
            landingPage.Name = "landingPage";
            landingPage.Size = new Size(435, 479);
            landingPage.TabIndex = 0;
            landingPage.Text = "Root";
            landingPage.UseVisualStyleBackColor = true;
            // 
            // landingView
            // 
            landingView.BackColor = SystemColors.Control;
            landingView.BackgroundImage = (Image)resources.GetObject("landingView.BackgroundImage");
            landingView.BackgroundImageLayout = ImageLayout.Stretch;
            landingView.Dock = DockStyle.Fill;
            landingView.Location = new Point(0, 0);
            landingView.Margin = new Padding(0);
            landingView.Name = "landingView";
            landingView.Size = new Size(435, 479);
            landingView.TabIndex = 4;
            // 
            // datManagerPage
            // 
            datManagerPage.Controls.Add(datSummaryView);
            datManagerPage.Location = new Point(0, 20);
            datManagerPage.Margin = new Padding(0);
            datManagerPage.Name = "datManagerPage";
            datManagerPage.Size = new Size(435, 479);
            datManagerPage.TabIndex = 2;
            datManagerPage.Text = "Grid";
            datManagerPage.UseVisualStyleBackColor = true;
            // 
            // datSummaryView
            // 
            datSummaryView.Dock = DockStyle.Fill;
            datSummaryView.Location = new Point(0, 0);
            datSummaryView.Margin = new Padding(0);
            datSummaryView.Name = "datSummaryView";
            datSummaryView.Size = new Size(435, 479);
            datSummaryView.TabIndex = 3;
            // 
            // datDetailsView
            // 
            datDetailsView.Dock = DockStyle.Fill;
            datDetailsView.Location = new Point(0, 0);
            datDetailsView.Margin = new Padding(0);
            datDetailsView.Name = "datDetailsView";
            datDetailsView.Size = new Size(509, 499);
            datDetailsView.TabIndex = 5;
            // 
            // mainControlsView
            // 
            mainControlsView.BackColor = SystemColors.Control;
            mainControlsView.Dock = DockStyle.Fill;
            mainControlsView.Location = new Point(0, 499);
            mainControlsView.Margin = new Padding(0);
            mainControlsView.Name = "mainControlsView";
            mainControlsView.Size = new Size(949, 60);
            mainControlsView.TabIndex = 11;
            // 
            // MainView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainLayoutPanel);
            Margin = new Padding(0);
            Name = "MainView";
            Size = new Size(949, 559);
            mainLayoutPanel.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            landingPage.ResumeLayout(false);
            datManagerPage.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private datinate.app.MainControlsView mainControlsView;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TabPage landingPage;
        private datinate.app.LandingView landingView;
        private System.Windows.Forms.TabPage datManagerPage;
        private datinate.app.DatSummaryView datSummaryView;
        private datinate.app.DatDetailsView datDetailsView;
        private BorderlessTabControl tabControl;
    }
}
