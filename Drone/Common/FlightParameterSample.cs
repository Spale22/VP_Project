using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class FlightParameterSample
    {
        [DataMember]
        public double LinearAccelerationX { get; set; }
        [DataMember]
        public double LinearAccelerationY { get; set; }
        [DataMember]
        public double LinearAccelerationZ { get; set; }
        [DataMember]
        public double WindSpeed { get; set; }
        [DataMember]
        public double WindAngle { get; set; }
        [DataMember]
        public DateTime Time { get; set; }

        public FlightParameterSample(double linearAccelerationX, double linearAccelerationY, double linearAccelerationZ, double windSpeed, double windAngle, DateTime time)
        {
            LinearAccelerationX = linearAccelerationX;
            LinearAccelerationY = linearAccelerationY;
            LinearAccelerationZ = linearAccelerationZ;
            WindSpeed = windSpeed;
            WindAngle = windAngle;
            Time = time;
        }
    }
}
