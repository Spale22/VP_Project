using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Client
{
    public class CSVReader : IDisposable
    {
        bool disposed = false;
        StreamReader sr;
        public CSVReader(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file {filePath} was not found.");
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filePath);
            sr = new StreamReader(filePath);
        }

        ~CSVReader()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (sr != null)
                    {
                        sr.Dispose();
                        sr = null;
                    }
                }
                // Dispose unmanaged resources if any
                disposed = true;
            }
        }

        public List<FlightParameterSample> ReadSamples(string filePath)
        {
            int maxRowsRead = int.Parse(ConfigurationManager.AppSettings["MaxRowsRead"]);
            List<FlightParameterSample> samples = new List<FlightParameterSample>(maxRowsRead);


            sr.ReadLine(); // Skip header line
            string line;
            for (int i = 0; i < maxRowsRead; i++)
            {
                line = sr.ReadLine();

                if (line == null)
                    continue;

                string[] parameters = line.Split(',');

                if (!(parameters.Length > 0 && parameters.Length < 22))
                    continue;

                DateTime time = DateTime.Now;
                double wind_speed = double.Parse(parameters[1]);
                double wind_angle = double.Parse(parameters[2]);
                double linear_acceleration_x = double.Parse(parameters[18]);
                double linear_acceleration_y = double.Parse(parameters[19]);
                double linear_acceleration_z = double.Parse(parameters[20]);

                samples.Add(new FlightParameterSample(linear_acceleration_x, linear_acceleration_y, linear_acceleration_z, wind_speed, wind_angle, time));
            }

            return samples;
        }
    }
}
