namespace Microsoft.Azure.Monitoring.SmartDetectors.ProjectType
{
    partial class WizardForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardForm));
            this.templatesPanel = new System.Windows.Forms.Panel();
            this.MetricButton = new System.Windows.Forms.RadioButton();
            this.ApplicationInsightsButton = new System.Windows.Forms.RadioButton();
            this.LogAnalyticsButton = new System.Windows.Forms.RadioButton();
            this.EmptyButton = new System.Windows.Forms.RadioButton();            
            this.templateTextBox = new System.Windows.Forms.TextBox();
            this.templateInfo = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.templatesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // templatesPanel
            // 
            this.templatesPanel.BackColor = System.Drawing.SystemColors.Window;
            this.templatesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.templatesPanel.Controls.Add(this.MetricButton);
            this.templatesPanel.Controls.Add(this.ApplicationInsightsButton);
            this.templatesPanel.Controls.Add(this.LogAnalyticsButton);
            this.templatesPanel.Controls.Add(this.EmptyButton);
            this.templatesPanel.Controls.Add(this.templateTextBox);
            this.templatesPanel.Location = new System.Drawing.Point(12, 12);
            this.templatesPanel.Name = "templatesPanel";
            this.templatesPanel.Size = new System.Drawing.Size(270, 208);
            this.templatesPanel.TabIndex = 5;
            // 
            // MetricButton
            // 
            this.MetricButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.MetricButton.BackColor = System.Drawing.Color.White;
            this.MetricButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.MetricButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.MetricButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MetricButton.Image = ((System.Drawing.Image)(resources.GetObject("MetricButton.Image")));
            this.MetricButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.MetricButton.Location = new System.Drawing.Point(9, 119);
            this.MetricButton.Name = "MetricButton";
            this.MetricButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.MetricButton.Size = new System.Drawing.Size(70, 75);
            this.MetricButton.TabIndex = 17;
            this.MetricButton.Text = "Metric\r\n ";
            this.MetricButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MetricButton.UseVisualStyleBackColor = false;
            this.MetricButton.Click += new System.EventHandler(this.MetricButton_Click);
            // 
            // ApplicationInsightsButton
            // 
            this.ApplicationInsightsButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.ApplicationInsightsButton.BackColor = System.Drawing.Color.White;
            this.ApplicationInsightsButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.ApplicationInsightsButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.ApplicationInsightsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ApplicationInsightsButton.Image = ((System.Drawing.Image)(resources.GetObject("ApplicationInsightsButton.Image")));
            this.ApplicationInsightsButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.ApplicationInsightsButton.Location = new System.Drawing.Point(177, 30);
            this.ApplicationInsightsButton.Name = "ApplicationInsightsButton";
            this.ApplicationInsightsButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.ApplicationInsightsButton.Size = new System.Drawing.Size(70, 75);
            this.ApplicationInsightsButton.TabIndex = 16;
            this.ApplicationInsightsButton.Text = "Application Insights";
            this.ApplicationInsightsButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ApplicationInsightsButton.UseVisualStyleBackColor = false;
            this.ApplicationInsightsButton.Click += new System.EventHandler(this.ApplicationInsightsButton_Click);
            // 
            // LogAnalyticsButton
            // 
            this.LogAnalyticsButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.LogAnalyticsButton.BackColor = System.Drawing.Color.White;
            this.LogAnalyticsButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.LogAnalyticsButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.LogAnalyticsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LogAnalyticsButton.Image = ((System.Drawing.Image)(resources.GetObject("LogAnalyticsButton.Image")));
            this.LogAnalyticsButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.LogAnalyticsButton.Location = new System.Drawing.Point(90, 30);
            this.LogAnalyticsButton.Name = "LogAnalyticsButton";
            this.LogAnalyticsButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.LogAnalyticsButton.Size = new System.Drawing.Size(70, 75);
            this.LogAnalyticsButton.TabIndex = 15;
            this.LogAnalyticsButton.Text = "Log Analytics";
            this.LogAnalyticsButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.LogAnalyticsButton.UseVisualStyleBackColor = false;
            this.LogAnalyticsButton.Click += new System.EventHandler(this.LogAnalyticsButton_Click);
            // 
            // EmptyButton
            // 
            this.EmptyButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.EmptyButton.BackColor = System.Drawing.Color.White;
            this.EmptyButton.Checked = true;
            this.EmptyButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.EmptyButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.EmptyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EmptyButton.Image = ((System.Drawing.Image)(resources.GetObject("EmptyButton.Image")));
            this.EmptyButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.EmptyButton.Location = new System.Drawing.Point(9, 30);
            this.EmptyButton.Name = "EmptyButton";
            this.EmptyButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.EmptyButton.Size = new System.Drawing.Size(70, 75);
            this.EmptyButton.TabIndex = 13;
            this.EmptyButton.TabStop = true;
            this.EmptyButton.Text = "Empty\r\n ";
            this.EmptyButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.EmptyButton.UseVisualStyleBackColor = false;
            this.EmptyButton.Click += new System.EventHandler(this.EmptyButton_Click);
            // 
            // templateTextBox
            // 
            this.templateTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.templateTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.templateTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.templateTextBox.Location = new System.Drawing.Point(9, 11);
            this.templateTextBox.Name = "templateTextBox";
            this.templateTextBox.Size = new System.Drawing.Size(212, 13);
            this.templateTextBox.TabIndex = 1;
            this.templateTextBox.Text = "Smart Detector Templates";
            // 
            // templateInfo
            // 
            this.templateInfo.BackColor = System.Drawing.SystemColors.Control;
            this.templateInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.templateInfo.Location = new System.Drawing.Point(301, 12);
            this.templateInfo.Multiline = true;
            this.templateInfo.Name = "templateInfo";
            this.templateInfo.Size = new System.Drawing.Size(326, 119);
            this.templateInfo.TabIndex = 5;            
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(485, 225);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(69, 25);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(560, 225);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(69, 25);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // WizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(641, 261);
            this.Controls.Add(this.templateInfo);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.templatesPanel);
            this.Name = "WizardForm";
            this.ShowIcon = false;
            this.Text = "New Smart Detector";
            this.templatesPanel.ResumeLayout(false);
            this.templatesPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox templateTextBox;
        private System.Windows.Forms.Panel templatesPanel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox templateInfo;
        private System.Windows.Forms.RadioButton EmptyButton;
        private System.Windows.Forms.RadioButton LogAnalyticsButton;
        private System.Windows.Forms.RadioButton MetricButton;
        private System.Windows.Forms.RadioButton ApplicationInsightsButton;
    }
}