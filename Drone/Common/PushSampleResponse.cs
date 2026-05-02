using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class PushSampleResponse
    {
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public int SampleNumber { get; set; }

        [DataMember]
        public SessionStatus SessionStatus { get; set; }

        public PushSampleResponse(string status, string message, int sampleNumber, SessionStatus sessionStatus)
        {
            Status = status;
            Message = message;
            SampleNumber = sampleNumber;
            SessionStatus = sessionStatus;
        }
    }
}
