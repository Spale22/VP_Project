using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class PushSampleResponse
    {
        [DataMember]
        public AckStatus Status { get; private set; }

        [DataMember]
        public string Message { get; private set; }

        [DataMember]
        public int SampleNumber { get; private set; }

        [DataMember]
        public SessionStatus SessionStatus { get; private set; }

        [DataMember]
        public List<WarningDTO> Warnings { get; private set; }

        public PushSampleResponse(AckStatus status, string message, int sampleNumber, SessionStatus sessionStatus, List<WarningDTO> warnings = null)
        {
            Status = status;
            Message = message;
            SampleNumber = sampleNumber;
            SessionStatus = sessionStatus;
            Warnings = warnings ?? new List<WarningDTO>();
        }
    }
}
