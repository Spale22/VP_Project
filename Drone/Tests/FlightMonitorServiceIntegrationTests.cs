using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Services;
using System;
using System.ServiceModel;

namespace Tests
{
    [TestClass]
    public class FlightMonitorServiceIntegrationTests
    {
        private FlightMonitorService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new FlightMonitorService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                _service?.Dispose();
            }
            catch { }
        }

        #region Session Lifecycle Tests

        [TestMethod]
        public void StartSession_WithValidMetadata_ShouldInitializeSession()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartSession_WhenSessionAlreadyActive_ShouldThrowException()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);
            _service.StartSession(metadata); // Should throw
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void StartSession_WithInvalidMetadata_ShouldThrowFault()
        {
            var invalidMetadata = new SessionMetaData(
                Guid.NewGuid(),
                "",  // Invalid: empty
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(invalidMetadata);
        }

        [TestMethod]
        public void EndSession_WithActiveSession_ShouldCompleteSession()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);
            _service.EndSession();
        }

        [TestMethod]
        public void EndSession_WithoutActiveSession_ShouldNotThrow()
        {
            _service.EndSession();
        }

        #endregion

        #region Sample Processing Tests

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PushSample_WithoutActiveSession_ShouldThrowException()
        {
            var sample = new FlightParameterSample(1.0, 2.0, 3.0, 5.0, 45.0, 1.0);
            _service.PushSample(sample);
        }

        [TestMethod]
        public void PushSample_WithValidSample_ShouldReturnAck()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                10,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);
            var sample = new FlightParameterSample(1.0, 2.0, 3.0, 5.0, 45.0, 1.0);
            var response = _service.PushSample(sample);

            Assert.AreEqual(AckStatus.ACK, response.Status);
            Assert.AreEqual(1, response.SampleNumber);
            Assert.AreEqual(SessionStatus.IN_PROGRESS, response.SessionStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void PushSample_WithInvalidSample_ShouldThrowFault()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);
            var invalidSample = new FlightParameterSample(1.0, 2.0, 3.0, 5.0, 361.0, 1.0); // WindAngle > 360
            _service.PushSample(invalidSample);
        }

        [TestMethod]
        public void PushSample_MultipleValidSamples_ShouldIncrementSampleCount()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);

            for (int i = 0; i < 10; i++)
            {
                var sample = new FlightParameterSample(
                    Math.Sin(i * 0.1),
                    Math.Cos(i * 0.1),
                    Math.Sin(i * 0.2),
                    5.0 + i * 0.1,
                    45.0,
                    i
                );

                var response = _service.PushSample(sample);
                Assert.AreEqual(i + 1, response.SampleNumber);
                Assert.AreEqual(AckStatus.ACK, response.Status);
            }
        }

        [TestMethod]
        public void FullSessionFlow_StartPushEnd_ShouldComplete()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                10,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);

            for (int i = 0; i < 10; i++)
            {
                var sample = new FlightParameterSample(
                    10.0 * Math.Sin(i * 0.1),
                    15.0 * Math.Cos(i * 0.1),
                    20.0,
                    5.0 + i * 0.1,
                    45.0 + i * 2,
                    i
                );

                var response = _service.PushSample(sample);
                Assert.AreEqual(AckStatus.ACK, response.Status);
            }

            _service.EndSession();
        }

        #endregion

        #region Dispose/Cleanup Tests

        [TestMethod]
        public void Dispose_ShouldCleanupResources()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                5,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);
            _service.Dispose();

            // After dispose, should not be able to push samples
            var sample = new FlightParameterSample(1.0, 2.0, 3.0, 5.0, 45.0, 1.0);
            try
            {
                _service.PushSample(sample);
                Assert.Fail("Should have thrown exception after disposal");
            }
            catch (ObjectDisposedException)
            {
                // Expected
            }
            catch (InvalidOperationException)
            {
                // Also acceptable - session no longer active
            }
        }

        #endregion

        #region Exception Handling Tests

        [TestMethod]
        public void StartSession_WhenValidationFails_ShouldCatchAndThrow()
        {
            var invalidMetadata = new SessionMetaData(
                Guid.NewGuid(),
                null,
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            try
            {
                _service.StartSession(invalidMetadata);
                Assert.Fail("Should have thrown FaultException");
            }
            catch (FaultException<ValidationFault>)
            {
                // Expected
            }
        }

        [TestMethod]
        public void PushSample_WhenValidationFails_ShouldThrowAndNotifyStorage()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);

            var invalidSample = new FlightParameterSample(1000.0, 2000.0, 3000.0, 5.0, 45.0, 1.0);

            try
            {
                _service.PushSample(invalidSample);
                Assert.Fail("Should have thrown FaultException");
            }
            catch (FaultException<ValidationFault> ex)
            {
                Assert.IsTrue(ex.Detail.Message.Contains("acceleration"));
            }
        }

        #endregion

        #region Session State Tests

        [TestMethod]
        public void SessionState_AfterStart_ShouldAcceptSamples()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);

            var sample = new FlightParameterSample(1.0, 2.0, 3.0, 5.0, 45.0, 1.0);
            var response = _service.PushSample(sample);

            Assert.IsNotNull(response);
            Assert.AreEqual(AckStatus.ACK, response.Status);
        }

        [TestMethod]
        public void SessionState_AfterEnd_ShouldNotAcceptSamples()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );

            _service.StartSession(metadata);
            _service.EndSession();

            var sample = new FlightParameterSample(1.0, 2.0, 3.0, 5.0, 45.0, 1.0);

            try
            {
                _service.PushSample(sample);
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        #endregion
    }
}
