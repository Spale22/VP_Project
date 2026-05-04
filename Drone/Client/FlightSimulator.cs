using Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Client
{
    public class FlightSimulator
    {
        List<FlightParameterSample> _samples;
        SessionMetaData _sessionMetadata;

        public FlightSimulator(List<FlightParameterSample> samples, SessionMetaData sessionMetadata)
        {
            _samples = samples;
            _sessionMetadata = sessionMetadata;
        }

        public void SimulateFlight(IFlightMonitorService service)
        {
            bool sessionStarted = false;
            int samplesSent = 0;
            try
            {
                service.StartSession(_sessionMetadata);
                sessionStarted = true;
                Console.WriteLine($"[Flight Simulator] Session started. Sending {_samples.Count} samples...");

                foreach (var sample in _samples)
                {
                    try
                    {
                        PushSampleResponse response = service.PushSample(sample);
                        samplesSent++;
                        Console.WriteLine($"[Sample #{response.SampleNumber}] Status: {response.Status} | Message: {response.Message} | SessionStatus: {response.SessionStatus}");
                    }
                    catch (FaultException<ValidationFault> validationEx)
                    {
                        Console.WriteLine($"[Sample #{samplesSent + 1}] Status: NACK | Error: {validationEx.Detail.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Sample #{samplesSent + 1}] Status: NACK | Unexpected error: {ex.Message}");
                        break;
                    }
                }

                Console.WriteLine($"[Flight Simulator] Transfer completed. {samplesSent}/{_samples.Count} samples sent successfully.");
            }
            catch (FaultException<ValidationFault> ex)
            {
                Console.WriteLine($"[Flight Simulator] Validation error during session start: {ex.Detail.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Flight Simulator] An unexpected error occurred.\n{ex.Message}");
            }
            finally
            {
                try
                {
                    if (sessionStarted)
                        service.EndSession();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Flight Simulator] Error while ending session: {ex.Message}");
                }
            }
        }
    }
}
