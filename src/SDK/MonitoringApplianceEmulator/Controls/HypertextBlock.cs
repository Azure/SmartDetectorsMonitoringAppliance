//-----------------------------------------------------------------------
// <copyright file="HypertextBlock.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    /// <summary>
    /// An extension of <see cref="TextBlock"/> control that displays hypertext. The hypertext should be transfered using the <see cref="Hypertext"/> dependency property.
    /// </summary>
    public class HypertextBlock : TextBlock
    {
        /// <summary>
        /// The hypertext dependency property. This text may include multiple hyperlinks in valid HTML A tags.
        /// For example, '<a href="https://msdn.microsoft.com">Developer Network</a>' will be replaced with a hyperlink with display text of "Developers Network".
        /// </summary>
        public static readonly DependencyProperty HypertextProperty = DependencyProperty.Register(
            "Hypertext",
            typeof(string),
            typeof(HypertextBlock),
            new FrameworkPropertyMetadata(
                string.Empty,
                new PropertyChangedCallback(OnHypertextPropertyChanged)),
            HypertextValidateCallback);

        private static readonly Regex HtmlHyperlinkRegex = new Regex("<a [^>]*href[ ]*=[ ]*(?<href>(?:'.*?')|(?:\".*?\"))[ ]*>(?<linkText>[^<]*)</a>");

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the hypertext.
        /// </summary>
        public string Hypertext
        {
            get
            {
                return (string)this.GetValue(HypertextProperty);
            }

            set
            {
                this.SetValue(HypertextProperty, value);
            }
        }

        #endregion

        /// <summary>
        /// Occurs when the <see cref="Hypertext"/> dependency property was changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object</param>
        /// <param name="eventArgs">The event args</param>
        private static void OnHypertextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!(dependencyObject is HypertextBlock hypertextTextBlock))
            {
                throw new ArgumentException($"The dependency object must be of type {typeof(HypertextBlock)}, but it is from type {dependencyObject.GetType()}.", nameof(dependencyObject));
            }

            if (!(eventArgs.NewValue is string hypertext))
            {
                throw new ArgumentException($"The new value must be of type {typeof(string)}, but it is from type {eventArgs.NewValue.GetType()}.", nameof(eventArgs));
            }

            Match match = HtmlHyperlinkRegex.Match(hypertext);

            // In case there are no links in the text
            if (!match.Success)
            {
                hypertextTextBlock.Inlines.Add(hypertext);
            }

            Match previousMatch = null;
            while (match.Success)
            {
                // First, add text before the current match
                int startIndexOfTextBeforeMatch = previousMatch == null ?
                    0 :
                    previousMatch.Index + previousMatch.Length;

                int endIndexOfTextBeforeMatch = previousMatch == null ?
                    match.Index :
                    match.Index - (previousMatch.Index + previousMatch.Length);

                string textBeforeMatch = hypertext.Substring(startIndexOfTextBeforeMatch, endIndexOfTextBeforeMatch);

                hypertextTextBlock.Inlines.Add(textBeforeMatch);

                // Second, convert the match to hyperlink and add it
                string url = match.Groups["href"].Value.Trim('\'', '\"');
                string linkDisplayText = match.Groups["linkText"].Value;

                Hyperlink link = new Hyperlink { IsEnabled = true };
                link.Inlines.Add(linkDisplayText);
                link.NavigateUri = new Uri(url);
                link.RequestNavigate += (sender, args) => Process.Start(args.Uri.ToString());

                hypertextTextBlock.Inlines.Add(link);

                // Continue to next match
                previousMatch = match;
                match = match.NextMatch();
            }

            // If there is text after the last found match, add it as well
            if (previousMatch != null)
            {
                int remainingTextStartIndex = previousMatch.Index + previousMatch.Length;
                //// int remainingTextEndIndex = hypertext.Length - 1 - remainingTextStartIndex;
                string textAfterLaftMatch = hypertext.Substring(remainingTextStartIndex);

                hypertextTextBlock.Inlines.Add(textAfterLaftMatch);
            }
        }

        /// <summary>
        /// Validates the new assigned value for <see cref="Hypertext"/> dependency property.
        /// </summary>
        /// <param name="value">The new assigned value</param>
        /// <returns>True if valid, otherwise false</returns>
        private static bool HypertextValidateCallback(object value)
        {
            return value != null;
        }
    }
}
