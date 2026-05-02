using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IFlightMonitorService
    {
        [OperationContract]
        void StartSession();
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(FlightParameterSample sample);
        [OperationContract]
        void EndSession();
    }
}
