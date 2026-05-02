# VP_Project

Drone telemetry simulation and monitoring built with a three-project .NET Framework 4.7.2 solution.

The repository contains:

- a console server that hosts a WCF `FlightMonitorService`
- a console client that reads telemetry from CSV and simulates a flight session
- a shared library with service contracts, DTOs, enums, and validation faults

## Overview

The client loads sample telemetry from `Client/Resources/TestDataSet.csv`, builds a session metadata payload, and sends the samples to the server over `net.tcp`.

The server validates the session and each sample, analyzes the telemetry, logs warnings and errors, and stores accepted and rejected rows as CSV files.

## Solution Structure

- `Drone/Client` - console client and CSV reader
- `Drone/Server` - WCF host and processing services
- `Drone/Common` - shared contracts and data models

## Requirements

- Visual Studio with .NET Framework 4.7.2 support
- Windows with WCF `net.tcp` enabled
- Permission to write to the output folders under `bin`

## How It Works

1. `Server` starts a WCF host at `net.tcp://localhost:4005/FlightMonitor`.
2. `Client` opens a channel to that endpoint and calls `StartSession`.
3. `Client` reads telemetry rows from the CSV file and sends samples one by one.
4. `Server` validates each sample, runs analysis, logs events, and writes accepted samples to the measurements output.
5. Rejected samples are written to a separate rejects CSV with the validation reason.

## Running The Project

1. Open `Drone/Drone.sln` in Visual Studio.
2. Build the solution.
3. Start the `Server` project first.
4. Start the `Client` project after the server is running.

The client will read `Resources/TestDataSet.csv`, send the configured number of rows, and end the session automatically.

## Configuration

### Client

`Client/App.config`

- `MaxRowsRead` - limits how many telemetry rows are read from the CSV file
- `FlightMonitorService` endpoint - connects to `net.tcp://localhost:4005/FlightMonitor`

### Server

`Server/App.config`

- `W_threshold` - wind direction shift threshold
- `L_threshold` - lateral asymmetry threshold
- `DeviationPercentage` - allowed deviation from the running mean wind angle
- `MeasurementsOutputPath` - session CSV output directory
- `LogFilePath` - server log file path

## Input Data

The client expects the input CSV file to be available at `Client/Resources/TestDataSet.csv`.

Rows are parsed using these telemetry fields:

- `Time`
- `WindSpeed`
- `WindAngle`
- `LinearAccelerationX`
- `LinearAccelerationY`
- `LinearAccelerationZ`

Rows that cannot be parsed, exceed the row limit, or have an invalid column count are written to an overflow log in the client output directory.

## Output

The server creates these files under `Server/bin/Debug/Output` by default:

- `measurements_session_*.csv`
- `rejects_session_*.csv`

The server log is written to `Server/bin/Debug/Logs/FlightMonitorServerLog.txt`.

The client writes rejected CSV rows to `Client/bin/Debug/OverflowLogs/rejected_rows_*.csv`.

## Validation And Analysis

The server rejects malformed telemetry using WCF validation faults. Accepted samples are analyzed for:

- wind direction shifts
- wind angle out-of-band warnings
- lateral asymmetry

## Notes

- The solution targets .NET Framework 4.7.2 and uses `netTcpBinding`.
- If you change the server address or port, update both `Server/App.config` and `Client/App.config`.
- If you change the CSV schema, update the client parser and the shared telemetry contract together.
