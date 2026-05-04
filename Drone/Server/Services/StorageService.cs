using Common;
using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace Server.Services
{
    public class StorageService : IDisposable
    {
        StreamWriter _measurementsWriter;
        StreamWriter _rejectsWriter;
        string _outputPath;
        bool _disposed = false;

        public StorageService()
        {
            _outputPath = ConfigurationManager.AppSettings["MeasurementsOutputPath"];
            _outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _outputPath.TrimStart('\\'));

            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);
        }

        public void StartSession()
        {
            string sessionFileName = $"measurements_session_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string rejectsFileName = $"rejects_session_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string _currentSessionPath = Path.Combine(_outputPath, sessionFileName);
            string _currentRejectsPath = Path.Combine(_outputPath, rejectsFileName);

            _measurementsWriter = new StreamWriter(_currentSessionPath, append: false, encoding: Encoding.UTF8);
            _measurementsWriter.WriteLine("Time,LinearAccelerationX,LinearAccelerationY,LinearAccelerationZ,WindSpeed,WindAngle");
            _measurementsWriter.Flush();

            _rejectsWriter = new StreamWriter(_currentRejectsPath, append: false, encoding: Encoding.UTF8);
            _rejectsWriter.WriteLine("Time,LinearAccelerationX,LinearAccelerationY,LinearAccelerationZ,WindSpeed,WindAngle,RejectionReason");
            _rejectsWriter.Flush();

        }

        public void WriteSample(FlightParameterSample sample)
        {
            if (_disposed || _measurementsWriter == null)
                throw new ObjectDisposedException(nameof(StorageService));

            try
            {
                string line = $"{sample.FlightDuration}," +
                              $"{sample.LinearAccelerationX}," +
                              $"{sample.LinearAccelerationY}," +
                              $"{sample.LinearAccelerationZ}," +
                              $"{sample.WindSpeed}," +
                              $"{sample.WindAngle}";

                _measurementsWriter.WriteLine(line);
                _measurementsWriter.Flush();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error writing sample to storage: {ex.Message}", ex);
            }
        }

        public void WriteRejectedSample(FlightParameterSample sample, string reason)
        {
            if (_disposed || _rejectsWriter == null)
                throw new ObjectDisposedException(nameof(StorageService));

            try
            {
                string line = $"{sample?.FlightDuration}," +
                              $"{sample?.LinearAccelerationX}," +
                              $"{sample?.LinearAccelerationY}," +
                              $"{sample?.LinearAccelerationZ}," +
                              $"{sample?.WindSpeed}," +
                              $"{sample?.WindAngle}," +
                              $"\"{reason}\"";

                _rejectsWriter.WriteLine(line);
                _rejectsWriter.Flush();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error writing rejected sample: {ex.Message}", ex);
            }
        }
        public void EndSession()
        {
            if (_disposed)
                return;

            _measurementsWriter?.Flush();
            _measurementsWriter?.Dispose();
            _measurementsWriter = null;
            _rejectsWriter?.Flush();
            _rejectsWriter?.Dispose();
            _rejectsWriter = null;
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
                    _measurementsWriter?.Dispose();
                    _rejectsWriter?.Dispose();
                }
                _disposed = true;
            }
        }

        ~StorageService()
        {
            Dispose(false);
        }
    }
}
