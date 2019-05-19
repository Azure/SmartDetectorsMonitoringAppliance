//-----------------------------------------------------------------------
// <copyright file="SmartDetectorWizard.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.ProjectType
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TemplateWizard;
    using System.Windows.Forms;
    using EnvDTE;
    using System.IO;

    /// <summary>
    /// Defines the logic for the Smart Detector template wizard extension.
    /// </summary>
    class SmartDetectorWizard : IWizard
    {
        private WizardForm wizardForm;
        private SmartDetectorTemplateType selectedTemplate;
        private static readonly Dictionary<SmartDetectorTemplateType,List<string>> TypeItems = new Dictionary<SmartDetectorTemplateType, List<string>>()
        {
            {SmartDetectorTemplateType.Empty, new List<string>(){"Manifest.json", "EmptySmartDetector.cs", "EmptyAlert.cs"}},
            {SmartDetectorTemplateType.Metric, new List<string>(){"Manifest.json", "MetricSmartDetector.cs", "EmptyAlert.cs"}},
            {SmartDetectorTemplateType.LogAnalytics, new List<string>(){"Manifest.json", "LogSearchSmartDetector.cs", "LogSearchAlert.cs"}},
            {SmartDetectorTemplateType.ApplicationInsights, new List<string>(){"Manifest.json", "LogSearchSmartDetector.cs", "LogSearchAlert.cs"}},
        };

        /// <summary>
        /// Runs Smart Detector wizard logic before opening an item in the template.
        /// </summary>
        /// <param name="projectItem">The project item that will be opened.</param>
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        /// <summary>
        /// Runs Smart Detector wizard logic when a project has finished generating.
        /// </summary>
        /// <param name="project">The project that finished generating.</param>
        public void ProjectFinishedGenerating(Project project)
        {
        }

        /// <summary>
        /// Runs Smart Detector wizard logic when a project item has finished generating.
        /// </summary>
        /// <param name="projectItem">The project item that finished generating.</param>
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        /// <summary>
        /// Runs Smart Detector wizard logic when the wizard has completed all tasks.
        /// </summary>
        public void RunFinished()
        {
        }

        /// <summary>
        /// Runs Smart Detector wizard logic at the beginning of a template wizard run.
        /// Replaces the template parameters according to the selected template type.
        /// </summary>
        /// <param name="automationObject">The automation object being used by the template wizard.</param>
        /// <param name="replacementsDictionary">The list of standard parameters to be replaced.</param>
        /// <param name="runKind">A run kind indicating the type of wizard run</param>
        /// <param name="customParams">The custom parameters with which to perform parameter replacement in the project.</param>
        public void RunStarted(object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            try
            {
                wizardForm = new WizardForm();
                wizardForm.ShowDialog();

                if (wizardForm.DialogResult == DialogResult.Cancel)
                {
                    throw new WizardBackoutException();
                }

                selectedTemplate = wizardForm.SelectedTemplate;

                replacementsDictionary.Add("$detectorName$", "MySmartDetector");
                replacementsDictionary.Add("$alertName$", "MyAlert");

                switch (selectedTemplate)
                {
                    case SmartDetectorTemplateType.Empty:
                        replacementsDictionary.Add("$resourceType$", "VirtualMachine");
                        break;
                    case SmartDetectorTemplateType.LogAnalytics:
                        replacementsDictionary.Add("$dataType$", "Log Analytics");
                        replacementsDictionary.Add("$query$", "Perf " +
                                                              "| summarize Count=count() by bin(TimeGenerated, 1h) ");
                        replacementsDictionary.Add("$tableName$", "Perf");
                        replacementsDictionary.Add("$resourceType$", "VirtualMachine");
                        break;
                    case SmartDetectorTemplateType.ApplicationInsights:
                        replacementsDictionary.Add("$dataType$", "Application Insights");
                        replacementsDictionary.Add("$query$", "traces " +
                                                              "| summarize Count=count() by bin(timestamp, 1h) ");
                        replacementsDictionary.Add("$tableName$", "traces");
                        replacementsDictionary.Add("$resourceType$", "ApplicationInsights");
                        break;
                    case SmartDetectorTemplateType.Metric:
                        replacementsDictionary.Add("$dataType$", "Metric");
                        replacementsDictionary.Add("$resourceType$", "AzureStorage");
                        break;
                }
            }
            catch (WizardBackoutException)
            {
                var directoryToDelete = replacementsDictionary["$destinationdirectory$"];
                if (replacementsDictionary["$exclusiveproject$"] == "True")
                {
                    directoryToDelete = replacementsDictionary["$solutiondirectory$"];
                }

                if (Directory.Exists(directoryToDelete))
                {
                    Directory.Delete(directoryToDelete, true);
                }

                throw;
            }
        }

        /// <summary>
        /// Indicates whether the specified project item should be added to the Smart Detector project.
        /// </summary>
        /// <param name="filePath">The path to the project item.</param>
        /// <returns></returns>
        public bool ShouldAddProjectItem(string filePath)
        {
            if (TypeItems[selectedTemplate].Contains(filePath))
            {
                return true;
            }

            return false;
        }
    }
}
