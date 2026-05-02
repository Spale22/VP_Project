using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMetaData
    {
        [DataMember]
        public Guid SessionId { get; set; }

        [DataMember]
        public string SourceFileName { get; set; }

        [DataMember]
        public int ExpectedSampleCount { get; set; }

        [DataMember]
        public string[] TelemetryColumns { get; set; }

    }
}