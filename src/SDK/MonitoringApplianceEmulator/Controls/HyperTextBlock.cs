//-----------------------------------------------------------------------
// <copyright file="HyperTextBlock.cs" company="Microsoft Corporation">
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
    using System.Windows.Media.Imaging;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;

    /// <summary>
    /// An extension of <see cref="TextBlock"/> control that displays hypertext. The hypertext should be transferred using the <see cref="HyperText"/> dependency property.
    /// This control supports only links in the following format '<a href="https://msdn.microsoft.com">Developer Network</a>'.
    /// </summary>
    public class HyperTextBlock : TextBlock
    {
        /// <summary>
        /// The hypertext dependency property. This text may include multiple hyperlinks in valid HTML A tags.
        /// For example, '<a href="https://msdn.microsoft.com">Developer Network</a>' will be replaced with a hyperlink with display text of "Developers Network".
        /// </summary>
        public static readonly DependencyProperty HyperTextProperty = DependencyProperty.Register(
            "HyperText",
            typeof(string),
            typeof(HyperTextBlock),
            new FrameworkPropertyMetadata(
                string.Empty,
                OnHypertextPropertyChanged),
            HypertextValidateCallback);

        private static readonly Regex HtmlHyperlinkRegex = new Regex("<a [^>]*href[\\s]*=[\\s]*(?<href>(?:'.*?')|(?:\".*?\"))[ ]*>(?<linkText>[^<]*)</a>");

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperTextBlock"/> class
        /// </summary>
        public HyperTextBlock()
        {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = "Copy",
                Command = new CommandHandler(() =>
                {
                    Clipboard.SetText(this.HyperText);
                })
            });

            this.ContextMenu = contextMenu;
        }

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the hypertext.
        /// </summary>
        public string HyperText
        {
            get
            {
                return (string)this.GetValue(HyperTextProperty);
            }

            set
            {
                this.SetValue(HyperTextProperty, value);
            }
        }

        #endregion

        /// <summary>
        /// Occurs when the <see cref="HyperText"/> dependency property was changed.
        /// This method replace all link patterns within the new assigned hypertext  <see cref="Hyperlink"/> elements.
        /// </summary>
        /// <param name="dependencyObject">The dependency object</param>
        /// <param name="eventArgs">The event args</param>
        private static void OnHypertextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!(dependencyObject is HyperTextBlock hypertextTextBlock))
            {
                throw new ArgumentException($"The dependency object must be of type {nameof(HyperTextBlock)}, but it is from type {dependencyObject.GetType().Name}.", nameof(dependencyObject));
            }

            if (!(eventArgs.NewValue is string hypertext))
            {
                throw new ArgumentException($"The new value must be of type {typeof(string).Name}, but it is from type {eventArgs.NewValue.GetType().Name}.", nameof(eventArgs));
            }

            Match match = HtmlHyperlinkRegex.Match(hypertext);

            // In case there are no links in the text, just add it
            if (!match.Success)
            {
                // Only for CAD scenarios, replace arrows ascii chart with image
                if (hypertext.StartsWith("↑", StringComparison.InvariantCulture))
                {
                    hypertext = hypertext.Trim('↑');

                    var upArrowImg = new BitmapImage(new Uri("pack://application:,,,/Media/up_arrow.png"));
                    Image image = new Image();
                    image.Source = upArrowImg;
                    image.Width = 15;
                    image.Height = 15;
                    image.Visibility = Visibility.Visible;
                    InlineUIContainer container = new InlineUIContainer(image);

                    hypertextTextBlock.Inlines.Add(container);
                    hypertextTextBlock.Inlines.Add(" ");
                }
                else if (hypertext.StartsWith("↓", StringComparison.InvariantCulture))
                {
                    hypertext = hypertext.Trim('↓');

                    var upArrowImg = new BitmapImage(new Uri("pack://application:,,,/Media/down_arrow.png"));
                    Image image = new Image();
                    image.Source = upArrowImg;
                    image.Width = 15;
                    image.Height = 15;
                    image.Visibility = Visibility.Visible;
                    InlineUIContainer container = new InlineUIContainer(image);

                    hypertextTextBlock.Inlines.Add(container);
                    hypertextTextBlock.Inlines.Add(" ");
                }

                hypertextTextBlock.Inlines.Add(hypertext);

                return;
            }

            hypertextTextBlock.Inlines.Clear();

            // Go over all link pattern matches and replace it with a hyperlink element
            Match previousMatch = null;
            while (match.Success)
            {
                // First, add text before the current match
                int startIndexOfTextBeforeMatch = previousMatch?.Index + previousMatch?.Length ?? 0;

                int lengthOfTextBeforeMatch = match.Index - (previousMatch?.Index + previousMatch?.Length) ?? match.Index;

                string textBeforeMatch = hypertext.Substring(startIndexOfTextBeforeMatch, lengthOfTextBeforeMatch);

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
                string textAfterLastMatch = hypertext.Substring(remainingTextStartIndex);

                if (!string.IsNullOrEmpty(textAfterLastMatch))
                {
                    hypertextTextBlock.Inlines.Add(textAfterLastMatch);
                }
            }
        }

        /// <summary>
        /// Validates the new assigned value for <see cref="HyperText"/> dependency property.
        /// </summary>
        /// <param name="value">The new assigned value</param>
        /// <returns>True if valid, otherwise false</returns>
        private static bool HypertextValidateCallback(object value)
        {
            return value != null;
        }
    }
}
