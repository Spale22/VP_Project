using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Client
{
    public class CSVReader : IDisposable
    {
        bool _disposed = false;
        StreamReader _csvReader;
        StreamWriter _overflowWriter;
        string _csvFilePath;

        public CSVReader(string filePath)
        {
            _csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filePath);

            if (!File.Exists(_csvFilePath))
                throw new FileNotFoundException($"The file {_csvFilePath} was not found.");

            _csvReader = new StreamReader(_csvFilePath);

            string overflowLogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OverflowLogs");
            if (!Directory.Exists(overflowLogDir))
                Directory.CreateDirectory(overflowLogDir);

            string overflowFilePath = Path.Combine(overflowLogDir, $"rejected_rows_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            _overflowWriter = new StreamWriter(overflowFilePath, false);
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

                if (parameters.Length < 21)
                {
                    _overflowWriter.WriteLine($"{lineNumber},\"{line}\",\"Invalid column count: expected ~21, got {parameters.Length}\"");
                    _overflowWriter.Flush();
                    continue;
                }

                try
                {
                    // Parse the required fields
                    // Indices: 0=Time, 1=WindSpeed, 2=WindAngle, 18=AccelX, 19=AccelY, 20=AccelZ
                    double time = double.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture);
                    double wind_speed = double.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture);
                    double wind_angle = double.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture);
                    double linear_acceleration_x = double.Parse(parameters[18], System.Globalization.CultureInfo.InvariantCulture);
                    double linear_acceleration_y = double.Parse(parameters[19], System.Globalization.CultureInfo.InvariantCulture);
                    double linear_acceleration_z = double.Parse(parameters[20], System.Globalization.CultureInfo.InvariantCulture);

                    samples.Add(new FlightParameterSample(linear_acceleration_x, linear_acceleration_y, linear_acceleration_z, wind_speed, wind_angle, time));
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

            return samples;
        }
    }
}
