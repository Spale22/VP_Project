using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Client
{
    public class CSVReader : IDisposable
    {
        private bool _disposed = false;
        private StreamReader _csvReader;
        private StreamWriter _overflowWriter;
        private string _csvFilePath;

        public CSVReader(string filePath)
        {
            _csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filePath);

            if (!File.Exists(_csvFilePath))
                throw new FileNotFoundException($"The file {_csvFilePath} was not found.");

            _csvReader = new StreamReader(_csvFilePath, System.Text.Encoding.UTF8);

            string overflowLogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OverflowLogs");
            if (!Directory.Exists(overflowLogDir))
                Directory.CreateDirectory(overflowLogDir);

            string overflowFilePath = Path.Combine(overflowLogDir, $"rejected_rows_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            _overflowWriter = new StreamWriter(overflowFilePath, false, System.Text.Encoding.UTF8);
            _overflowWriter.WriteLine("LineNumber,OriginalLine,ErrorMessage");
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
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_csvReader != null)
                    {
                        _csvReader.Dispose();
                        _csvReader = null;
                    }

                    if (_overflowWriter != null)
                    {
                        _overflowWriter.Flush();
                        _overflowWriter.Dispose();
                        _overflowWriter = null;
                    }
                }
                _disposed = true;
            }
        }

        public List<FlightParameterSample> ReadSamples(string filePath)
        {
            int maxRowsRead = int.Parse(ConfigurationManager.AppSettings["MaxRowsRead"]);
            int colTime = int.Parse(ConfigurationManager.AppSettings["CSVColumnTime"] ?? "0");
            int colWindSpeed = int.Parse(ConfigurationManager.AppSettings["CSVColumnWindSpeed"] ?? "1");
            int colWindAngle = int.Parse(ConfigurationManager.AppSettings["CSVColumnWindAngle"] ?? "2");
            int colAccelX = int.Parse(ConfigurationManager.AppSettings["CSVColumnAccelX"] ?? "18");
            int colAccelY = int.Parse(ConfigurationManager.AppSettings["CSVColumnAccelY"] ?? "19");
            int colAccelZ = int.Parse(ConfigurationManager.AppSettings["CSVColumnAccelZ"] ?? "20");

            List<FlightParameterSample> samples = new List<FlightParameterSample>(maxRowsRead);

            string line = _csvReader.ReadLine();
            int lineNumber = 1;
            int sampleCount = 0;

            while (sampleCount < maxRowsRead)
            {
                line = _csvReader.ReadLine();
                lineNumber++;

                if (line == null)
                    break;

                string[] parameters = line.Split(',');

                int maxIndex = Math.Max(Math.Max(Math.Max(colTime, colWindSpeed), Math.Max(colWindAngle, colAccelX)), Math.Max(colAccelY, colAccelZ));
                if (parameters.Length < maxIndex + 1)
                {
                    _overflowWriter.WriteLine($"{lineNumber},\"{line}\",\"Invalid column count: expected at least {maxIndex + 1}, got {parameters.Length}\"");
                    _overflowWriter.Flush();
                    continue;
                }

                try
                {
                    // Parse the required fields using configurable indices
                    double time = double.Parse(parameters[colTime], System.Globalization.CultureInfo.InvariantCulture);
                    double windSpeed = double.Parse(parameters[colWindSpeed], System.Globalization.CultureInfo.InvariantCulture);
                    double windAngle = double.Parse(parameters[colWindAngle], System.Globalization.CultureInfo.InvariantCulture);
                    double linearAccelerationX = double.Parse(parameters[colAccelX], System.Globalization.CultureInfo.InvariantCulture);
                    double linearAccelerationY = double.Parse(parameters[colAccelY], System.Globalization.CultureInfo.InvariantCulture);
                    double linearAccelerationZ = double.Parse(parameters[colAccelZ], System.Globalization.CultureInfo.InvariantCulture);

                    samples.Add(new FlightParameterSample(linearAccelerationX, linearAccelerationY, linearAccelerationZ, windSpeed, windAngle, time));
                    sampleCount++;
                }
                catch (Exception ex)
                {
                    _overflowWriter.WriteLine($"{lineNumber},\"{line}\",\"Parsing error: {ex.Message}\"");
                    _overflowWriter.Flush();
                    continue;
                }
            }

            while ((line = _csvReader.ReadLine()) != null)
            {
                lineNumber++;
                _overflowWriter.WriteLine($"{lineNumber},\"{line}\",\"Row exceeds maxRowsRead limit ({maxRowsRead})\"");
            }
            _overflowWriter.Flush();

            return samples;
        }
    }
}
