using System;
using System.ServiceModel;

namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(Services.FlightMonitorService));
            try
            {
                host.Open();
                Console.WriteLine("Server is running ...");
                Console.WriteLine("Press any key to shut down the server...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
            }
            finally
            {
                try
                {
                    host.Close();
                    Console.WriteLine("Server has been shut down.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing server: {ex.Message}");
                    host.Abort();
                }
            }
        }
    }
}
