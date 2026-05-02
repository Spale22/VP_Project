using Common;
using System;

namespace Server.Services
{
    public class WindDirectionShiftEventArgs : EventArgs
    {
        public double DeltaWindAngle { get; set; }
        public WindRotationDirection Direction { get; set; }
        public double FlightDuration { get; set; }
    }

    public class OutOfBandWarningEventArgs : EventArgs
    {
        public double WindAngle { get; set; }
        public double RunningMean { get; set; }
        public DeviationType Deviation { get; set; }
        public double FlightDuration { get; set; }
    }

    public class LateralAsymmetryEventArgs : EventArgs
    {
        public double Wasym { get; set; }
        public LateralDirection Direction { get; set; }
        public double FlightDuration { get; set; }
    }

    public class SampleReceivedEventArgs : EventArgs
    {
        public int SampleCount { get; set; }
        public SessionStatus Status { get; set; }
    }

    public class TransferStatusEventArgs : EventArgs
    {
        public SessionStatus Status { get; set; }
        public double FlightDuration { get; set; }
    }
}
