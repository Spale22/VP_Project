using Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Client
{
    public class FlightSimulator
    {
        List<FlightParameterSample> _samples;
        public FlightSimulator(List<FlightParameterSample> samples)
        {
            _samples = samples;
        }

        public void SimulateFlight(IFlightMonitorService service)
        {
            service.StartSession();
            try
            {
                foreach (var sample in _samples)
                {
                    service.PushSample(sample);
                }
            }
            catch (FaultException<ValidationFault> ex)
            {
                Console.WriteLine($"Validation error: {ex.Detail.Message}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during flight simulation.\n{ex.Message}");
            }
            finally
            {
                service.EndSession();
            }
        }
    }
}
