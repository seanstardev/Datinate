using datinate.app;
using System.Drawing;

namespace com.RADIO.Datinate.App.View.problemList {
    partial class ProblemListView {
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
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProblemListForm));
            tabControl = new System.Windows.Forms.TabControl();
            unreadablePage = new System.Windows.Forms.TabPage();
            unreadableUI = new ProblemListUI();
            duplicatesPage = new System.Windows.Forms.TabPage();
            duplicatesUI = new ProblemListUI();
            tabControl.SuspendLayout();
            unreadablePage.SuspendLayout();
            duplicatesPage.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(unreadablePage);
            tabControl.Controls.Add(duplicatesPage);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(331, 417);
            tabControl.TabIndex = 1;
            // 
            // unreadablePage
            // 
            unreadablePage.Controls.Add(unreadableUI);
            unreadablePage.Location = new System.Drawing.Point(4, 24);
            unreadablePage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            unreadablePage.Name = "unreadablePage";
            unreadablePage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            unreadablePage.Size = new System.Drawing.Size(323, 389);
            unreadablePage.TabIndex = 0;
            unreadablePage.Text = "Unreadable";
            unreadablePage.UseVisualStyleBackColor = true;
            // 
            // unreadableUI
            // 
            unreadableUI.Dock = System.Windows.Forms.DockStyle.Fill;
            unreadableUI.Location = new System.Drawing.Point(4, 3);
            unreadableUI.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            unreadableUI.Name = "unreadableUI";
            unreadableUI.Size = new System.Drawing.Size(315, 383);
            unreadableUI.TabIndex = 0;
            // 
            // duplicatesPage
            // 
            duplicatesPage.Controls.Add(duplicatesUI);
            duplicatesPage.Location = new System.Drawing.Point(4, 24);
            duplicatesPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            duplicatesPage.Name = "duplicatesPage";
            duplicatesPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            duplicatesPage.Size = new System.Drawing.Size(323, 389);
            duplicatesPage.TabIndex = 1;
            duplicatesPage.Text = "Duplicates";
            duplicatesPage.UseVisualStyleBackColor = true;
            // 
            // duplicatesUI
            // 
            duplicatesUI.Dock = System.Windows.Forms.DockStyle.Fill;
            duplicatesUI.Location = new System.Drawing.Point(4, 3);
            duplicatesUI.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            duplicatesUI.Name = "duplicatesUI";
            duplicatesUI.Size = new System.Drawing.Size(315, 383);
            duplicatesUI.TabIndex = 0;
            // 
            // ProblemListForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(331, 417);
            Controls.Add(tabControl);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);

            MinimumSize = new System.Drawing.Size(347, 456);
            Name = "ProblemListForm";
            Text = "Problem List";
            tabControl.ResumeLayout(false);
            unreadablePage.ResumeLayout(false);
            duplicatesPage.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage unreadablePage;
        private System.Windows.Forms.TabPage duplicatesPage;
        private ProblemListUI unreadableUI;
        private ProblemListUI duplicatesUI;
    }
}
