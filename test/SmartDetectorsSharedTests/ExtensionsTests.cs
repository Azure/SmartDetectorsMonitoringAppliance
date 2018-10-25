//-----------------------------------------------------------------------
// <copyright file="ExtensionsTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void WhenInvokingAsInterpolatedStringThenTheResultsAreAsExpected()
        {
            Person p = new Person()
            {
                Name = "John",
                Salary = 511.5748,
                BirthDate = new DateTime(2001, 11, 23, 19, 41, 30)
            };

            // Property matching and format
            string format = "{Name} was born in {BirthDate:u}, and {{ earns {Salary}$ a day";
            string s = StringExtensions.EvaluateInterpolatedString(format, p);
            Assert.AreEqual("John was born in 2001-11-23 19:41:30Z, and { earns 511.5748$ a day", s);

            // Conditional
            p.Count = 1;
            format = "Found {Count} affected machine{(Count == 1 ? \"\" : \"s\")}";
            s = StringExtensions.EvaluateInterpolatedString(format, p);
            Assert.AreEqual("Found 1 affected machine", s);
            p.Count = 4;
            s = StringExtensions.EvaluateInterpolatedString(format, p);
            Assert.AreEqual("Found 4 affected machines", s);

            // Invalid property
            format = "{Name} was born in {BirthDateX:D}, and earns {Salary:F3}$ a day";
            try
            {
                StringExtensions.EvaluateInterpolatedString(format, p);
                Assert.Fail("A FormatException should have been thrown");
            }
            catch (Exception e) when (e.Message.Contains("BirthDateX"))
            {
            }

            // Invalid braces syntax
            format = "{Name} was born in {BirthDate:D}, and earns {Salary:F3}$ { a day";
            try
            {
                StringExtensions.EvaluateInterpolatedString(format, p);
                Assert.Fail("A FormatException should have been thrown");
            }
            catch (Exception e) when (e.Message.Contains("Missing close delimiter"))
            {
            }
        }

        public class Person
        {
            public string Name { get; set; }

            public double Salary { get; set; }

            public DateTime BirthDate { get; set; }

            public int Count { get; set; }
        }
    }
}
