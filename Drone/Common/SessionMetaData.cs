using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMetaData
    {
        [DataMember]
        public Guid SessionId { get; private set; }

        [DataMember]
        public string SourceFileName { get; private set; }

        [DataMember]
        public int ExpectedSampleCount { get; private set; }

        [DataMember]
        public string[] TelemetryColumns { get; private set; }

        public SessionMetaData(Guid sessionId, string sourceFileName, int expectedSampleCount, string[] telemetryColumns)
        {
            SessionId = sessionId;
            SourceFileName = sourceFileName;
            ExpectedSampleCount = expectedSampleCount;
            TelemetryColumns = telemetryColumns;
        }

    }
}