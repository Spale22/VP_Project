using Common;
using System;
using System.ServiceModel;

namespace Client
{
    internal class Client
    {
        static void Main(string[] args)
        {
            try
            {
                ChannelFactory<IFlightMonitorService> cf = new ChannelFactory<IFlightMonitorService>("FlightMonitorService");

                IFlightMonitorService proxy = cf.CreateChannel();

                using (CSVReader reader = new CSVReader("TestDataSet.csv"))
                {
                    var samples = reader.ReadSamples("TestDataSet.csv");
                    SessionMetaData sessionMetadata = new SessionMetaData
                    {
                        SessionId = Guid.NewGuid(),
                        SourceFileName = "TestDataSet.csv",
                        ExpectedSampleCount = samples.Count,
                        TelemetryColumns = new[]
                            {
                                "LinearAccelerationX",
                                "LinearAccelerationY",
                                "LinearAccelerationZ",
                                "WindSpeed",
                                "WindAngle",
                                "Time"
                            }
                    };

                    FlightSimulator fs = new FlightSimulator(samples, sessionMetadata);

                    Console.WriteLine("Starting flight simulation...");
                    fs.SimulateFlight(proxy);
                    Console.WriteLine("Flight simulation completed.");
                }

                ((IClientChannel)proxy).Close();
                cf.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
