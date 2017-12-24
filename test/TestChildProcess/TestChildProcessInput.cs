//-----------------------------------------------------------------------
// <copyright file="TestChildProcessInput.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestChildProcess
{
    /// <summary>
    /// The child process input type
    /// </summary>
    public class TestChildProcessInput
    {
        public const int ExpectedIntValue = 2347;

        public const string ExpectedStringValue = "Premature optimization";

        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public RunMode RunMode { get; set; }
    }
}