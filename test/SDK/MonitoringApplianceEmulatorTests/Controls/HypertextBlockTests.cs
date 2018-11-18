//-----------------------------------------------------------------------
// <copyright file="HypertextBlockTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Controls
{
    using System.Linq;
    using System.Text;
    using System.Windows.Documents;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HypertextBlockTests
    {
        [TestMethod]
        public void WhenAssigningHypertextThatStartsWithLinkToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "<a href=\"https://en.wikipedia.org/wiki/Michael_Jordan\">Michael</a> will always be better than <a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron</a>. And that's it!";
            string expectedDisplayText = "Michael will always be better than Lebron. And that's it!";

            var hypertextBlock = new HypertextBlock();

            // Act
            hypertextBlock.Hypertext = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.Hypertext, $"Unexpected value of {nameof(hypertextBlock.Hypertext)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(2, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
            Assert.AreEqual(hyperlinkInlines.First().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/Michael_Jordan", "Unexpected URL in first link.");
            Assert.AreEqual(hyperlinkInlines.Last().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/LeBron_James", "Unexpected URL in second link.");
        }

        public void WhenAssigningHypertextStartsWithPlainTextToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "No matter what most people say... <a href=\"https://en.wikipedia.org/wiki/Michael_Jordan\">Michael</a> will always be better than <a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron</a>. And that's it!";
            string expectedDisplayText = "No matter what most people say... Michael will always be better than Lebron. And that's it!";

            var hypertextBlock = new HypertextBlock();

            // Act
            hypertextBlock.Hypertext = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.Hypertext, $"Unexpected value of {nameof(hypertextBlock.Hypertext)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(2, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
            Assert.AreEqual(hyperlinkInlines.First().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/Michael_Jordan", "Unexpected URL in first link.");
            Assert.AreEqual(hyperlinkInlines.Last().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/LeBron_James", "Unexpected URL in second link.");
        }

        private static string GetHypertextBoxDisplayText(HypertextBlock hypertextBlock)
        {
            StringBuilder displayText = new StringBuilder();
            foreach (Inline inline in hypertextBlock.Inlines)
            {
                if (inline.GetType() == typeof(Run))
                {
                    displayText.Append(((Run)inline).Text);
                }
                else if (inline.GetType() == typeof(Hyperlink))
                {
                    displayText.Append(((Run)((Hyperlink)inline).Inlines.FirstInline).Text);
                }
            }

            return displayText.ToString();
        }
    }
}
