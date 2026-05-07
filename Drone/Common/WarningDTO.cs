using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class WarningDTO
    {
        [DataMember]
        public WarningType WarningType { get; private set; }

        [DataMember]
        public double FlightDuration { get; private set; }

        [DataMember]
        public double? DeltaWindAngle { get; private set; }

        [DataMember]
        public WindRotationDirection? Direction { get; private set; }

        [DataMember]
        public double? WindAngle { get; private set; }

        [DataMember]
        public double? RunningMean { get; private set; }

        [DataMember]
        public DeviationType? Deviation { get; private set; }

        [DataMember]
        public double? Wasym { get; private set; }
        [DataMember]
        public LateralDirection? LateralDirection { get; private set; }

        public WarningDTO(WarningType warningType, double flightDuration, double deltaWindAngle, WindRotationDirection direction)
        {
            WarningType = warningType;
            FlightDuration = flightDuration;
            DeltaWindAngle = deltaWindAngle;
            Direction = direction;
        }

        public WarningDTO(WarningType warningType, double flightDuration, double windAngle, double runningMean, DeviationType deviation)
        {
            WarningType = warningType;
            FlightDuration = flightDuration;
            WindAngle = windAngle;
            RunningMean = runningMean;
            Deviation = deviation;
        }
        public WarningDTO(WarningType warningType, double flightDuration, double wasym, LateralDirection lateralDirection)
        {
            WarningType = warningType;
            FlightDuration = flightDuration;
            LateralDirection = lateralDirection;
            Wasym = wasym;
        }
    }
}
