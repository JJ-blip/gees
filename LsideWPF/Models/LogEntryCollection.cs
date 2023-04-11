using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LsideWPF.Models
{
    public class LogEntryCollection : ObservableCollection<LogEntry>
    {
        public LogEntryCollection() { }

        public void Add(ObservableCollection<LogEntry> logEntries)
        {
            foreach (LogEntry logEntry in logEntries) { this.Add(logEntry); }
        }

        internal void Add(IEnumerable<LogEntry> filtered)
        {
            foreach (LogEntry logEntry in filtered) { this.Add(logEntry); }
        }
    }
}