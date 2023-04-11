using LsideWPF.Common;
using LsideWPF.Models;
using System.Collections.Generic;
using System.Data;

namespace LsideWPF.Services
{
    public interface ILandingLoggerService
    {
        LogEntryCollection GetLandingLogEntries();

        void Add(List<LogEntry> logEntries);

        void Add(LogEntry logEntry);

        DataTable GetLandingLogData();

        FlightParameters GetLastLandingFromCVS();
    }
}
