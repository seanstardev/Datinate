namespace com.RADIO.Datinate.App.View.compare {
    partial class CompareView {
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
            tableLayoutPanel1 = new TableLayoutPanel();
            splitContainer1 = new SplitContainer();
            leftUI = new datinate.app.CompareUI();
            splitContainer2 = new SplitContainer();
            middleUI = new datinate.app.CompareUI();
            rightUI = new datinate.app.CompareUI();
            panel7 = new Panel();
            clearBtn = new Button();
            filterUI1 = new datinate.app.FilterUI();
            compareBtn = new Button();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            panel7.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(splitContainer1, 0, 0);
            tableLayoutPanel1.Controls.Add(panel7, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1046, 495);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(4, 3);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(leftUI);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1038, 458);
            splitContainer1.SplitterDistance = 365;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // leftUI
            // 
            leftUI.BorderStyle = BorderStyle.FixedSingle;
            leftUI.Dock = DockStyle.Fill;
            leftUI.Enabled = false;
            leftUI.Location = new Point(0, 0);
            leftUI.Margin = new Padding(5, 3, 5, 3);
            leftUI.Name = "leftUI";
            leftUI.Size = new Size(365, 458);
            leftUI.TabIndex = 0;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(middleUI);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(rightUI);
            splitContainer2.Size = new Size(668, 458);
            splitContainer2.SplitterDistance = 291;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // middleUI
            // 
            middleUI.BorderStyle = BorderStyle.FixedSingle;
            middleUI.Dock = DockStyle.Fill;
            middleUI.Enabled = false;
            middleUI.Location = new Point(0, 0);
            middleUI.Margin = new Padding(5, 3, 5, 3);
            middleUI.Name = "middleUI";
            middleUI.Size = new Size(291, 458);
            middleUI.TabIndex = 0;
            // 
            // rightUI
            // 
            rightUI.BorderStyle = BorderStyle.FixedSingle;
            rightUI.Dock = DockStyle.Fill;
            rightUI.Enabled = false;
            rightUI.Location = new Point(0, 0);
            rightUI.Margin = new Padding(5, 3, 5, 3);
            rightUI.Name = "rightUI";
            rightUI.Size = new Size(372, 458);
            rightUI.TabIndex = 0;
            // 
            // panel7
            // 
            panel7.Controls.Add(clearBtn);
            panel7.Controls.Add(filterUI1);
            panel7.Controls.Add(compareBtn);
            panel7.Dock = DockStyle.Fill;
            panel7.Location = new Point(4, 467);
            panel7.Margin = new Padding(4, 3, 4, 3);
            panel7.Name = "panel7";
            panel7.Size = new Size(1038, 25);
            panel7.TabIndex = 1;
            // 
            // clearBtn
            // 
            clearBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            clearBtn.Location = new Point(4, -1);
            clearBtn.Margin = new Padding(4, 3, 4, 3);
            clearBtn.Name = "clearBtn";
            clearBtn.Size = new Size(88, 27);
            clearBtn.TabIndex = 2;
            clearBtn.Text = "Clear";
            clearBtn.UseVisualStyleBackColor = true;
            clearBtn.Click += ClearBtn_Click;
            // 
            // filterUI1
            // 
            filterUI1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            filterUI1.Location = new Point(100, 0);
            filterUI1.Margin = new Padding(5, 3, 5, 3);
            filterUI1.Name = "filterUI1";
            filterUI1.Size = new Size(261, 24);
            filterUI1.TabIndex = 1;
            // 
            // compareBtn
            // 
            compareBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            compareBtn.Location = new Point(366, -1);
            compareBtn.Margin = new Padding(4, 3, 4, 3);
            compareBtn.Name = "compareBtn";
            compareBtn.Size = new Size(88, 27);
            compareBtn.TabIndex = 0;
            compareBtn.Text = "Compare";
            compareBtn.UseVisualStyleBackColor = true;
            compareBtn.Click += OnCompareClick;
            // 
            // CompareView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "CompareView";
            Size = new Size(1046, 495);
            tableLayoutPanel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            panel7.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private datinate.app.CompareUI leftUI;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private datinate.app.CompareUI middleUI;
        private datinate.app.CompareUI rightUI;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Button clearBtn;
        private datinate.app.FilterUI filterUI1;
        private System.Windows.Forms.Button compareBtn;
    }
}
