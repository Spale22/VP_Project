using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMetaData
    {
        [DataMember]
        public int SessionId { get; set; }
    }
}
