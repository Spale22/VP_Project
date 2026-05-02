using Common;
using System;
using System.Linq;
using System.ServiceModel;

namespace Server.Services
{
    public class ValidationService
    {
        private static readonly string[] RequiredTelemetryColumns =
        {
            "LinearAccelerationX",
            "LinearAccelerationY",
            "LinearAccelerationZ",
            "WindSpeed",
            "WindAngle",
            "Time"
        };

        public void ValidateSessionMetadata(SessionMetaData metadata)
        {
            if (metadata == null)
                throw new FaultException<ValidationFault>(new ValidationFault("Session metadata cannot be null."));

            if (string.IsNullOrWhiteSpace(metadata.SourceFileName))
                throw new FaultException<ValidationFault>(new ValidationFault("SourceFileName is required."));

            if (metadata.ExpectedSampleCount <= 0)
                throw new FaultException<ValidationFault>(new ValidationFault("ExpectedSampleCount must be greater than zero."));

            if (metadata.TelemetryColumns == null || metadata.TelemetryColumns.Length == 0)
                throw new FaultException<ValidationFault>(new ValidationFault("TelemetryColumns are required."));

            var missingColumns = RequiredTelemetryColumns
                .Except(metadata.TelemetryColumns, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (missingColumns.Length > 0)
                throw new FaultException<ValidationFault>(new ValidationFault($"TelemetryColumns are incomplete. Missing: {string.Join(", ", missingColumns)}."));
        }

        public void ValidateSample(FlightParameterSample sample)
        {
            if (sample == null)
                throw new FaultException<ValidationFault>(new ValidationFault("Sample cannot be null."));

            ValidateFinite(sample.LinearAccelerationX, nameof(sample.LinearAccelerationX));
            ValidateFinite(sample.LinearAccelerationY, nameof(sample.LinearAccelerationY));
            ValidateFinite(sample.LinearAccelerationZ, nameof(sample.LinearAccelerationZ));
            ValidateFinite(sample.WindSpeed, nameof(sample.WindSpeed));
            ValidateFinite(sample.WindAngle, nameof(sample.WindAngle));
            ValidateFinite(sample.FlightDuration, nameof(sample.FlightDuration));

            if (sample.WindSpeed <= 0)
                throw new FaultException<ValidationFault>(new ValidationFault("WindSpeed must be greater than zero."));

            if (Math.Abs(sample.LinearAccelerationX) > 1000 ||
                Math.Abs(sample.LinearAccelerationY) > 1000 ||
                Math.Abs(sample.LinearAccelerationZ) > 1000)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Linear acceleration values are outside the expected range."));
            }

            if (Math.Abs(sample.WindAngle) > 3600)
                throw new FaultException<ValidationFault>(new ValidationFault("WindAngle is outside the expected range."));

            if (sample.FlightDuration < 0)
                throw new FaultException<ValidationFault>(new ValidationFault("FlightDuration is outside the expected range."));
        }

        private static void ValidateFinite(double value, string fieldName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new FaultException<ValidationFault>(new ValidationFault($"{fieldName} contains an invalid numeric value."));
        }
    }
}
