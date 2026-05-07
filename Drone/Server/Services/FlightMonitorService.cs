using Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class FlightMonitorService : IFlightMonitorService, IDisposable
    {
        private SessionMetaData _sessionMetadata;
        private ValidationService _validationService;
        private FlightParameterAnalyzingService _analyzingService;
        private LoggerService _loggerService;
        private StorageService _storageService;
        private List<WarningDTO> _currentSampleWarnings;
        private bool _sessionActive = false;
        private bool _disposed = false;

        public FlightMonitorService()
        {
            _validationService = new ValidationService();
            _analyzingService = new FlightParameterAnalyzingService();
            _loggerService = new LoggerService();
            _storageService = new StorageService();

            _analyzingService.OnWindDirectionShift += HandleWindDirectionShift;
            _analyzingService.OnOutOfBandWarning += HandleOutOfBandWarning;
            _analyzingService.OnLateralAsymmetry += HandleLateralAsymmetry;
            _analyzingService.OnSampleReceived += HandleSampleReceived;
            _analyzingService.OnTransferStarted += HandleTransferStarted;
            _analyzingService.OnTransferCompleted += HandleTransferCompleted;
            _analyzingService.OnWarningRaised += HandleWarningRaised;
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

                _analyzingService.RaiseTransferStarted(metadata.ExpectedSampleCount);
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
                _currentSampleWarnings = new List<WarningDTO>();

                _validationService.ValidateSample(sample);

                _analyzingService.AnalyzeSample(sample);

                _storageService.WriteSample(sample);

                _loggerService.LogMessage($"Sample processed: Flight duration: {sample.FlightDuration:F2} WindAngle={sample.WindAngle:F2}°");

                int currentCount = _analyzingService.GetSampleCount();
                SessionStatus sessionStatus = SessionStatus.IN_PROGRESS;

                return new PushSampleResponse(AckStatus.ACK, "Sample processed successfully", currentCount, sessionStatus, _currentSampleWarnings);
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
            finally
            {
                _currentSampleWarnings = null;
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
        ~FlightMonitorService()
        {
            Dispose(false);
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
                    try
                    {
                        if (_sessionActive)
                            EndSession();
                    }
                    catch (Exception ex)
                    {
                        _loggerService.LogError($"Error during disposal: {ex.Message}");
                    }

                    _analyzingService.OnWindDirectionShift -= HandleWindDirectionShift;
                    _analyzingService.OnOutOfBandWarning -= HandleOutOfBandWarning;
                    _analyzingService.OnLateralAsymmetry -= HandleLateralAsymmetry;
                    _analyzingService.OnSampleReceived -= HandleSampleReceived;
                    _analyzingService.OnTransferStarted -= HandleTransferStarted;
                    _analyzingService.OnTransferCompleted -= HandleTransferCompleted;
                    _analyzingService.OnWarningRaised -= HandleWarningRaised;

                    _storageService?.Dispose();
                    _loggerService?.Dispose();
                }
                _disposed = true;
            }
        }

        private void HandleWindDirectionShift(object sender, WindDirectionShiftEventArgs args)
        {
            var warning = new WarningDTO(WarningType.WindDirectionShift, args.FlightDuration, args.DeltaWindAngle, args.Direction);

            _currentSampleWarnings?.Add(warning);
            _loggerService.LogWarning($"Wind Direction Shift detected: Delta={args.DeltaWindAngle:F2}° Direction={args.Direction} Flight duration: {args.FlightDuration:F2}");
        }

        private void HandleOutOfBandWarning(object sender, OutOfBandWarningEventArgs args)
        {
            var warning = new WarningDTO(WarningType.OutOfBand, args.FlightDuration, args.WindAngle, args.RunningMean, args.Deviation);

            _currentSampleWarnings?.Add(warning);
            _loggerService.LogWarning($"Out-of-Band Warning: WindAngle={args.WindAngle:F2}° is {args.Deviation} expected (mean={args.RunningMean:F2}°) Flight duration: {args.FlightDuration:F2}");
        }

        private void HandleLateralAsymmetry(object sender, LateralAsymmetryEventArgs args)
        {
            var warning = new WarningDTO(WarningType.LateralAsymmetry, args.FlightDuration, args.Wasym, args.Direction);
            _currentSampleWarnings?.Add(warning);
            _loggerService.LogWarning($"Lateral Asymmetry Warning: Wasym={args.Wasym:F4} Direction={args.Direction} Flight duration: {args.FlightDuration:F2}");
        }

        private void HandleSampleReceived(object sender, SampleReceivedEventArgs args)
        {
            _loggerService.LogMessage($"Sample received: #{args.SampleCount} Session Status: {args.Status}");
        }

        private void HandleTransferStarted(object sender, TransferStartedEventArgs args)
        {
            _loggerService.LogMessage($"Transfer started. Expected samples: {args.ExpectedSampleCount}");
        }

        private void HandleTransferCompleted(object sender, TransferCompletedEventArgs args)
        {
            _loggerService.LogMessage($"Transfer completed. Total samples: {args.TotalSamplesReceived}, Session Status: {args.FinalStatus}");
        }

        private void HandleWarningRaised(object sender, WarningRaisedEventArgs args)
        {
            _loggerService.LogWarning($"Warning raised - Type: {args.WarningType}, Duration: {args.Warning.FlightDuration:F2}");
        }

    }
}
