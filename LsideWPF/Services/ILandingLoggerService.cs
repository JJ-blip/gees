namespace LsideWPF.Services
{
    using System.Collections.Generic;
    using System.Data;
    using LsideWPF.Models;

    public interface ILandingLoggerService
    {
        LogEntryCollection GetLandingLogEntries();

        void Add(List<LogEntry> logEntries);

        void Add(LogEntry logEntry);

        LogEntry GetLastLanding();
    }
}
