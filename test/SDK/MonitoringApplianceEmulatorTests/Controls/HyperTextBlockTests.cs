//-----------------------------------------------------------------------
// <copyright file="HyperTextBlockTests.cs" company="Microsoft Corporation">
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
    public class HyperTextBlockTests
    {
        [TestMethod]
        public void WhenAssigningHypertextThatStartsWithLinkToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "<a href=\"https://en.wikipedia.org/wiki/Michael_Jordan\">Michael</a> will always be better than <a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron</a>. And that's it!";
            string expectedDisplayText = "Michael will always be better than Lebron. And that's it!";

            var hypertextBlock = new HyperTextBlock();

            // Act
            hypertextBlock.HyperText = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.HyperText, $"Unexpected value of {nameof(hypertextBlock.HyperText)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(2, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
            Assert.AreEqual(hyperlinkInlines.First().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/Michael_Jordan", "Unexpected URL in first link.");
            Assert.AreEqual(hyperlinkInlines.Last().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/LeBron_James", "Unexpected URL in second link.");
        }

        [TestMethod]
        public void WhenAssigningHypertextThatStartsWithPlainTextToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "No matter what most people say... <a href=\"https://en.wikipedia.org/wiki/Michael_Jordan\">Michael</a> will always be better than <a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron</a>. And that's it!";
            string expectedDisplayText = "No matter what most people say... Michael will always be better than Lebron. And that's it!";

            var hypertextBlock = new HyperTextBlock();

            // Act
            hypertextBlock.HyperText = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.HyperText, $"Unexpected value of {nameof(hypertextBlock.HyperText)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(2, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
            Assert.AreEqual(hyperlinkInlines.First().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/Michael_Jordan", "Unexpected URL in first link.");
            Assert.AreEqual(hyperlinkInlines.Last().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/LeBron_James", "Unexpected URL in second link.");
        }

        [TestMethod]
        public void WhenAssigningHypertextThathEndsWithLinkToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "He will always be better than <a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron</a>";
            string expectedDisplayText = "He will always be better than Lebron";

            var hypertextBlock = new HyperTextBlock();

            // Act
            hypertextBlock.HyperText = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.HyperText, $"Unexpected value of {nameof(hypertextBlock.HyperText)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(1, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
            Assert.AreEqual(hyperlinkInlines.First().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/LeBron_James", "Unexpected URL in second link.");
        }

        [TestMethod]
        public void WhenAssigningHypertextWithOnlyPlainTextToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "No matter what most people say... Michael will always be better than Lebron. And that's it!";
            string expectedDisplayText = "No matter what most people say... Michael will always be better than Lebron. And that's it!";

            var hypertextBlock = new HyperTextBlock();

            // Act
            hypertextBlock.HyperText = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.HyperText, $"Unexpected value of {nameof(hypertextBlock.HyperText)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(0, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
        }

        [TestMethod]
        public void WhenAssigningHypertextWithOnlyLinkToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "<a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron</a>";
            string expectedDisplayText = "Lebron";

            var hypertextBlock = new HyperTextBlock();

            // Act
            hypertextBlock.HyperText = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.HyperText, $"Unexpected value of {nameof(hypertextBlock.HyperText)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(1, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
            Assert.AreEqual(hyperlinkInlines.First().NavigateUri.ToString(), "https://en.wikipedia.org/wiki/LeBron_James", "Unexpected URL in first link.");
        }

        [TestMethod]
        public void WhenAssigningHypertextWithLinkWithoutClosingTagToHypertextPropertyThenItIsBeingDisplayedExpectedly()
        {
            // Init
            string hypertext = "Hello, my name is <a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron";
            string expectedDisplayText = "Hello, my name is <a href=\"https://en.wikipedia.org/wiki/LeBron_James\">Lebron";

            var hypertextBlock = new HyperTextBlock();

            // Act
            hypertextBlock.HyperText = hypertext;

            // Assert
            Assert.AreEqual(hypertext, hypertextBlock.HyperText, $"Unexpected value of {nameof(hypertextBlock.HyperText)}");

            string resultDisplayText = GetHypertextBoxDisplayText(hypertextBlock);
            Assert.AreEqual(expectedDisplayText, resultDisplayText, "Unexpected display text");

            var hyperlinkInlines = hypertextBlock.Inlines.OfType<Hyperlink>().ToList();
            Assert.AreEqual(0, hyperlinkInlines.Count, "Unexpected number of hyperlinks was found in display text.");
        }

        private static string GetHypertextBoxDisplayText(HyperTextBlock hypertextBlock)
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
