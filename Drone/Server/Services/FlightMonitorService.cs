using Common;
using System;
using System.ServiceModel;

namespace Server.Services
{
    public class FlightMonitorService : IFlightMonitorService, IDisposable
    {
        SessionMetaData _sessionMetadata;
        ValidationService _validationService;
        FlightParameterAnalyzingService _analyzingService;
        LoggerService _loggerService;
        StorageService _storageService;
        bool _sessionActive = false;
        bool _disposed = false;

        public FlightMonitorService()
        {
            _validationService = new ValidationService();
            _analyzingService = new FlightParameterAnalyzingService();
            _loggerService = new LoggerService();
            _storageService = new StorageService();

            _analyzingService.OnWindDirectionShift += (sender, args) =>
            {
                _loggerService.LogWarning($"Wind Direction Shift detected: Delta={args.DeltaWindAngle:F2}° Direction={args.Direction} Flight duration: {args.FlightDuration:F2}");
            };

            _analyzingService.OnOutOfBandWarning += (sender, args) =>
            {
                _loggerService.LogWarning($"Out-of-Band Warning: WindAngle={args.WindAngle:F2}° is {args.Deviation} expected (mean={args.RunningMean:F2}°) Flight duration: {args.FlightDuration:F2}");
            };

            _analyzingService.OnLateralAsymmetry += (sender, args) =>
            {
                _loggerService.LogWarning($"Lateral Asymmetry Warning: Wasym={args.Wasym:F4} Direction={args.Direction} Flight duration: {args.FlightDuration:F2}");
            };

            _analyzingService.OnSampleReceived += (sender, args) =>
            {
                _loggerService.LogMessage($"Sample received: #{args.SampleCount} Status: {args.Status}");
            };
        }

        public void StartSession(SessionMetaData metadata)
        {
            if (_sessionActive)
            {
                _loggerService.LogError("Session already active. End current session before starting a new one.");
                throw new InvalidOperationException("Session already active.");
            }

            try
            {
                _validationService.ValidateSessionMetadata(metadata);
                _sessionMetadata = metadata;
                _sessionActive = true;
                _analyzingService.Reset();
                _storageService.StartSession();
                _loggerService.LogMessage($"Session started: Id={metadata.SessionId}, Source={metadata.SourceFileName}, Samples={metadata.ExpectedSampleCount}");
            }
            catch (Exception ex)
            {
                _sessionActive = false;
                _loggerService.LogError($"Error starting session: {ex.Message}");
                throw;
            }
        }

        public PushSampleResponse PushSample(FlightParameterSample sample)
        {
            if (!_sessionActive)
            {
                _loggerService.LogError("No active session. Call StartSession() first.");
                throw new InvalidOperationException("No active session.");
            }

            try
            {
                _validationService.ValidateSample(sample);

                _analyzingService.AnalyzeSample(sample);

                _storageService.WriteSample(sample);

                _loggerService.LogMessage($"Sample processed: Flight duration: {sample.FlightDuration:F2} WindAngle={sample.WindAngle:F2}°");

                int currentCount = _analyzingService.GetSampleCount();
                SessionStatus sessionStatus = SessionStatus.IN_PROGRESS;
                if (_sessionMetadata != null && _sessionMetadata.ExpectedSampleCount > 0 && currentCount >= _sessionMetadata.ExpectedSampleCount)
                {
                    sessionStatus = SessionStatus.COMPLETED;
                }

                return new PushSampleResponse("ACK", "Sample processed successfully", currentCount, sessionStatus);
            }
            catch (FaultException<ValidationFault> vf)
            {
                _loggerService.LogWarning($"Validation failed: {vf.Detail.Message}");
                _storageService.WriteRejectedSample(sample, vf.Detail.Message);
                throw;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error processing sample: {ex.Message}");
                _storageService.WriteRejectedSample(sample, ex.Message);
                throw;
            }
        }

        public void EndSession()
        {
            if (!_sessionActive)
            {
                _loggerService.LogWarning("No active session to end.");
                return;
            }

            try
            {
                _storageService.EndSession();
                _sessionActive = false;
                if (_sessionMetadata != null)
                    _loggerService.LogMessage($"Session completed: Id={_sessionMetadata.SessionId} Status=COMPLETED");
                _loggerService.LogMessage("Session ended.");
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error ending session: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_sessionActive)
                        EndSession();

                    _storageService?.Dispose();
                    _loggerService?.Dispose();
                }
                _disposed = true;
            }
        }

        ~FlightMonitorService()
        {
            Dispose(false);
        }
    }
}
