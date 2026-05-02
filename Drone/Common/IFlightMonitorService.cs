using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IFlightMonitorService
    {
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void StartSession(SessionMetaData metadata);
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        PushSampleResponse PushSample(FlightParameterSample sample);
        [OperationContract]
        void EndSession();
    }
}
