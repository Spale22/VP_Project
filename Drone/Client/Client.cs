using Common;
using System;
using System.Configuration;
using System.ServiceModel;

namespace Client
{
    public class Client
    {
        static void Main(string[] args)
        {
            ChannelFactory<IFlightMonitorService> cf = null;
            IFlightMonitorService proxy = null;
            string dataSetFileName = ConfigurationManager.AppSettings["DataSetFileName"];

            try
            {
                cf = new ChannelFactory<IFlightMonitorService>("FlightMonitorService");
                proxy = cf.CreateChannel();

                using (CSVReader reader = new CSVReader(dataSetFileName))
                {
                    var samples = reader.ReadSamples(dataSetFileName);

                    SessionMetaData sessionMetadata = new SessionMetaData(Guid.NewGuid(), dataSetFileName, samples.Count, new[] { "LinearAccelerationX", "LinearAccelerationY", "LinearAccelerationZ", "WindSpeed", "WindAngle", "Time" });

                    FlightSimulator fs = new FlightSimulator(samples, sessionMetadata);

                    Console.WriteLine("Starting flight simulation...");
                    fs.SimulateFlight(proxy);
                    Console.WriteLine("Flight simulation completed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (proxy != null)
                    {
                        var clientChannel = (IClientChannel)proxy;
                        if (clientChannel?.State != CommunicationState.Closed)
                            clientChannel?.Abort();
                    }

                    if (cf != null)
                    {
                        if (cf.State != CommunicationState.Closed)
                            cf.Abort();
                        cf.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing client: {ex.Message}");
                }

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
