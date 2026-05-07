using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Services;
using System;
using System.ServiceModel;

namespace Tests
{
    [TestClass]
    public class ValidationServiceTests
    {
        private ValidationService _validationService;

        [TestInitialize]
        public void Setup()
        {
            _validationService = new ValidationService();
        }

        #region WindAngle Validation Tests

        [TestMethod]
        public void ValidateSample_WindAngleAt0_ShouldPass()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, 0.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        public void ValidateSample_WindAngleAt360_ShouldPass()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, 360.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        public void ValidateSample_WindAngleNegative180_ShouldPass()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, -180.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_WindAngleAbove360_ShouldFail()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, 361.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_WindAngleBelow360Negative_ShouldFail()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, -361.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_WindAngle3600_ShouldFail()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, 3600.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        #endregion

        #region Linear Acceleration Validation Tests

        [TestMethod]
        public void ValidateSample_AccelerationWithinRange_ShouldPass()
        {
            var sample = new FlightParameterSample(100.0, 50.0, 75.0, 5.0, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_AccelerationXAboveLimit_ShouldFail()
        {
            var sample = new FlightParameterSample(1001.0, 50.0, 75.0, 5.0, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_AccelerationYAboveLimit_ShouldFail()
        {
            var sample = new FlightParameterSample(50.0, 1001.0, 75.0, 5.0, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_AccelerationZAboveLimit_ShouldFail()
        {
            var sample = new FlightParameterSample(50.0, 50.0, 1001.0, 5.0, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        public void ValidateSample_NegativeAccelerations_ShouldPass()
        {
            var sample = new FlightParameterSample(-100.0, -50.0, -75.0, 5.0, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        #endregion

        #region Wind Speed Validation Tests

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_WindSpeedZero_ShouldFail()
        {
            var sample = new FlightParameterSample(0, 0, 0, 0.0, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_WindSpeedNegative_ShouldFail()
        {
            var sample = new FlightParameterSample(0, 0, 0, -5.0, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        public void ValidateSample_WindSpeedPositive_ShouldPass()
        {
            var sample = new FlightParameterSample(0, 0, 0, 0.001, 45.0, 1.0);
            _validationService.ValidateSample(sample);
        }

        #endregion

        #region Flight Duration Tests

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSample_FlightDurationNegative_ShouldFail()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, 45.0, -1.0);
            _validationService.ValidateSample(sample);
        }

        [TestMethod]
        public void ValidateSample_FlightDurationZero_ShouldPass()
        {
            var sample = new FlightParameterSample(0, 0, 0, 5.0, 45.0, 0.0);
            _validationService.ValidateSample(sample);
        }

        #endregion

        #region Session Metadata Tests

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSessionMetadata_NullMetadata_ShouldFail()
        {
            _validationService.ValidateSessionMetadata(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSessionMetadata_NullSourceFileName_ShouldFail()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                null,
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );
            _validationService.ValidateSessionMetadata(metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSessionMetadata_ZeroExpectedSampleCount_ShouldFail()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                0,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );
            _validationService.ValidateSessionMetadata(metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ValidationFault>))]
        public void ValidateSessionMetadata_MissingRequiredColumns_ShouldFail()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY" }
            );
            _validationService.ValidateSessionMetadata(metadata);
        }

        [TestMethod]
        public void ValidateSessionMetadata_ValidMetadata_ShouldPass()
        {
            var metadata = new SessionMetaData(
                Guid.NewGuid(),
                "test.csv",
                100,
                new[] { "LinearAccelerationX", "LinearAccelerationY",
                        "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" }
            );
            _validationService.ValidateSessionMetadata(metadata);
        }

        #endregion
    }
}
