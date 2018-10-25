//-----------------------------------------------------------------------
// <copyright file="WizardForm.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.ProjectType
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// The Smart Detector Wizard Form
    /// </summary>
    public partial class WizardForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WizardForm"/> class 
        /// </summary>
        public WizardForm()
        {
            InitializeComponent();
            EmptyButton_Click(this.EmptyButton, new EventArgs());
        }

        /// <summary>
        /// Gets the selected template type
        /// </summary>  
        public SmartDetectorTemplateType SelectedTemplate { get; private set; }

        /// <summary>
        /// Occurs when the Empty template is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmptyButton_Click(object sender, EventArgs e)
        {
            this.SelectedTemplate = SmartDetectorTemplateType.Empty;
            templateInfo.Text = "An empty project template for creating Smart Detector. This template does not have any content in it.";
        }

        /// <summary>
        /// Occurs when the Log Analytics template is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogAnalyticsButton_Click(object sender, EventArgs e)
        {
            this.SelectedTemplate = SmartDetectorTemplateType.LogAnalytics;
            templateInfo.Text = "A project template for creating Smart Detector that detects alerts based on the result of a Log Analytics query.";
        }

        /// <summary>
        /// Occurs when the Application Insights template is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplicationInsightsButton_Click(object sender, EventArgs e)
        {
            this.SelectedTemplate = SmartDetectorTemplateType.ApplicationInsights;
            templateInfo.Text = "A project template for creating Smart Detector that detects alerts based on the result of an Application Insights query.";
        }

        /// <summary>
        /// Occurs when the Metric template is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetricButton_Click(object sender, EventArgs e)
        {
            this.SelectedTemplate = SmartDetectorTemplateType.Metric;
            templateInfo.Text = "A project template for creating Smart Detector that detects alerts based on a resource's metrics.";
        }
    }
}
