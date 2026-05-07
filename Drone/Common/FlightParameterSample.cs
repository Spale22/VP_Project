using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class FlightParameterSample
    {
        [DataMember]
        public double LinearAccelerationX { get; private set; }
        [DataMember]
        public double LinearAccelerationY { get; private set; }
        [DataMember]
        public double LinearAccelerationZ { get; private set; }
        [DataMember]
        public double WindSpeed { get; private set; }
        [DataMember]
        public double WindAngle { get; private set; }
        [DataMember]
        public double FlightDuration { get; private set; }

        public FlightParameterSample(double linearAccelerationX, double linearAccelerationY, double linearAccelerationZ, double windSpeed, double windAngle, double time)
        {
            LinearAccelerationX = linearAccelerationX;
            LinearAccelerationY = linearAccelerationY;
            LinearAccelerationZ = linearAccelerationZ;
            WindSpeed = windSpeed;
            WindAngle = windAngle;
            FlightDuration = time;
        }
    }
}
