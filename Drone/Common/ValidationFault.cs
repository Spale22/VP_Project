using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        private string _message;

        public ValidationFault(string message)
        {
            _message = message;
        }

        [DataMember]
        public string Message { get => _message; private set => _message = value; }
    }
}
