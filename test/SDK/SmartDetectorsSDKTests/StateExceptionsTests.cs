//-----------------------------------------------------------------------
// <copyright file="StateExceptionsTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSDKTests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StateExceptionsTests
    {
        [TestMethod]
        public void WhenStateTooBigExceptionIsSerializedAndDeserializedThenAllPropertiesContainExpectedValues()
        {
            StateTooBigException originalException = new StateTooBigException(1025, 1024);
            StateTooBigException deserializedException = null;
            var buffer = new byte[4096];

            using (var ms = new MemoryStream(buffer))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, originalException);
                ms.Position = 0;
                deserializedException = (StateTooBigException)formatter.Deserialize(ms);
            }

            Assert.AreEqual(originalException.MaxAllowedSerializedStateLength, deserializedException.MaxAllowedSerializedStateLength);
            Assert.AreEqual(originalException.SerializedStateLength, deserializedException.SerializedStateLength);
        }

        [TestMethod]
        public void WhenStateSerializationExceptionIsSerializedAndDeserializedThenAllPropertiesContainExpectedValues()
        {
            Exception innerException = new Exception("Hello");
            StateSerializationException originalException = new StateSerializationException(innerException);
            StateSerializationException deserializedException = null;
            var buffer = new byte[4096];

            using (var ms = new MemoryStream(buffer))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, originalException);
                ms.Position = 0;
                deserializedException = (StateSerializationException)formatter.Deserialize(ms);
            }

            Assert.AreEqual(originalException.InnerException.Message, deserializedException.InnerException.Message);
        }
    }
}
