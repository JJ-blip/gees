namespace LsideWPF.Services
{
    using System.Collections.Generic;
    using System.Data;

    public interface ILandingLoggerService
    {
        LogEntryCollection GetLandingLogEntries();

        void Add(List<LogEntry> logEntries);

        void Add(LogEntry logEntry);

        DataTable GetLandingLogData();

        FlightParameters GetLastLanding();
    }
}
