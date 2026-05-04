using Common;
using System;
using System.Configuration;

namespace Server.Services
{
    public class FlightParameterAnalyzingService
    {
        double _windAnglePrevious = double.NaN;
        double _windAngleSum = 0;
        int _sampleCount = 0;
        double _windAngleMean = 0;
        double _w_threshold;
        double _l_threshold;
        double _deviationPercentage;

        public event EventHandler<WindDirectionShiftEventArgs> OnWindDirectionShift;
        public event EventHandler<OutOfBandWarningEventArgs> OnOutOfBandWarning;
        public event EventHandler<LateralAsymmetryEventArgs> OnLateralAsymmetry;
        public event EventHandler<SampleReceivedEventArgs> OnSampleReceived;

        public FlightParameterAnalyzingService()
        {
            _w_threshold = double.Parse(ConfigurationManager.AppSettings["W_threshold"]);
            _l_threshold = double.Parse(ConfigurationManager.AppSettings["L_threshold"]);
            _deviationPercentage = double.Parse(ConfigurationManager.AppSettings["DeviationPercentage"]);
        }

        public void Reset()
        {
            _windAnglePrevious = double.NaN;
            _windAngleSum = 0;
            _sampleCount = 0;
            _windAngleMean = 0;
        }

        public void AnalyzeSample(FlightParameterSample sample)
        {
            UpdateRunningMean(sample);
            DetectWindDirectionShift(sample);
            DetectOutOfBandWarning(sample);
            DetectLateralAsymmetry(sample);

            _windAnglePrevious = sample.WindAngle;

            OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs
            {
                SampleCount = _sampleCount,
                Status = SessionStatus.IN_PROGRESS
            });
        }

        public int GetSampleCount()
        {
            return _sampleCount;
        }

        private void DetectWindDirectionShift(FlightParameterSample sample)
        { 
            double deltaWindAngle = sample.WindAngle - _windAnglePrevious;
            if (Math.Abs(deltaWindAngle) > _w_threshold)
            {
                WindRotationDirection direction = deltaWindAngle > 0 ? WindRotationDirection.Clockwise : WindRotationDirection.CounterClockwise;
                OnWindDirectionShift?.Invoke(this, new WindDirectionShiftEventArgs
                {
                    DeltaWindAngle = Math.Abs(deltaWindAngle),
                    Direction = direction,
                    FlightDuration = sample.FlightDuration
                });
            }
        }

        private void UpdateRunningMean(FlightParameterSample sample)
        {
            _windAngleSum += sample.WindAngle;
            _sampleCount++;
            _windAngleMean = _windAngleSum / _sampleCount;
        }

        private void DetectOutOfBandWarning(FlightParameterSample sample)
        {
            double lowerBound = _windAngleMean * (1 - _deviationPercentage);
            double upperBound = _windAngleMean * (1 + _deviationPercentage);

            if (sample.WindAngle < lowerBound)
            {
                OnOutOfBandWarning?.Invoke(this, new OutOfBandWarningEventArgs
                {
                    WindAngle = sample.WindAngle,
                    RunningMean = _windAngleMean,
                    Deviation = DeviationType.Below,
                    FlightDuration = sample.FlightDuration
                });
            }
            else if (sample.WindAngle > upperBound)
            {
                OnOutOfBandWarning?.Invoke(this, new OutOfBandWarningEventArgs
                {
                    WindAngle = sample.WindAngle,
                    RunningMean = _windAngleMean,
                    Deviation = DeviationType.Above,
                    FlightDuration = sample.FlightDuration
                });
            }
        }

        private void DetectLateralAsymmetry(FlightParameterSample sample)
        {
            double anorm = Math.Sqrt(
                sample.LinearAccelerationX * sample.LinearAccelerationX +
                sample.LinearAccelerationY * sample.LinearAccelerationY +
                sample.LinearAccelerationZ * sample.LinearAccelerationZ
            );
            if (anorm > 0)
            {
                double wasym = Math.Abs(sample.LinearAccelerationX) / anorm;
                if (wasym > _l_threshold)
                {
                    LateralDirection direction = sample.LinearAccelerationX > 0 ? LateralDirection.Right : LateralDirection.Left;
                    OnLateralAsymmetry?.Invoke(this, new LateralAsymmetryEventArgs
                    {
                        Wasym = wasym,
                        Direction = direction,
                        FlightDuration = sample.FlightDuration
                    });
                }
            }
        }
    }
}
