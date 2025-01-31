﻿namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using LsideWPF.Models;
    using Microsoft.Win32;
    using Serilog;

    public class LandingLogger : ILandingLoggerService
    {
        public void Add(LogEntry logEntry)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = false,
            };

            string path = this.MakeLogIfEmpty();

            // Append to the file.
            using (var stream = File.Open(path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                var options = new TypeConverterOptions { Formats = new[] { "dd/MM/yyyy HH:mm" } };
                csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);

                List<LogEntry> newRecord = new List<LogEntry>
                {
                    logEntry,
                };
                csv.WriteRecords(newRecord);
            }
        }

        public void Add(List<LogEntry> logEntryItems)
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string path = myDocs + "\\" + dir;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                DefaultExt = "*.csv",
                Filter = "CSV documents (.csv)|*.csv",
                FilterIndex = 2,
                InitialDirectory = path,
            };

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (Stream myStream = saveFileDialog1.OpenFile())
                {
                    if (myStream != null)
                    {
                        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture);

                        using (var writer = new StreamWriter(myStream))
                        using (var csv = new CsvWriter(writer, config))
                        {
                            var options = new TypeConverterOptions { Formats = new[] { "dd/MM/yyyy HH:mm" } };
                            csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);

                            List<LogEntry> logEntryList = new List<LogEntry>();
                            foreach (var item in logEntryItems)
                            {
                                logEntryList.Add(item);
                            }

                            csv.WriteRecords(logEntryList);
                        }
                    }
                }
            }
        }

        // return the most recent CSV row
        // return FlightParameters or null
        public LogEntry GetLastLanding()
        {
            var entries = this.GetLandingLogEntries();
            var last = entries.FirstOrDefault<LogEntry>() ?? new LogEntry();
            return last;
        }

        public LogEntryCollection GetLandingLogEntries()
        {
            try
            {
                CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    ReadingExceptionOccurred = this.IgnoreReadingException,
                };

                string path = GetPath();
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, config))
                {
                    var options = new TypeConverterOptions { Formats = new[] { "dd/MM/yyyy HH:mm" } };

                    csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    var records = csv.GetRecords<LogEntry>();
                    var sorted = records.OrderByDescending(entry => entry.Time);

                    LogEntryCollection logEntries = new LogEntryCollection
                {
                    sorted,
                };
                    return logEntries;
                }
            }
            catch (FileNotFoundException)
            {
                return new LogEntryCollection();
            }
        }

        internal static string GetPath()
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string filename = Properties.Settings.Default.LandingFile;

            // create if doesn't exist
            Directory.CreateDirectory(myDocs + "\\" + dir);
            string path = myDocs + "\\" + dir + "\\" + filename;

            return path;
        }

        internal string MakeLogIfEmpty()
        {
            string path = GetPath();
            if (!File.Exists(path))
            {
                try
                {
                    CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture);

                    using (var writer = new StreamWriter(path))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        var options = new TypeConverterOptions { Formats = new[] { "dd/MM/yyyy HH:mm:ss" } };
                        csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);

                        csv.WriteHeader<LogEntry>();
                        csv.NextRecord();

                        csv.WriteRecords(new List<LogEntry>());
                        csv.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"While creating Landing Logger file {path}", ex);
                }
            }

            return path;
        }

        private bool IgnoreReadingException(ReadingExceptionOccurredArgs args)
        {
            // discard the error row;
            return false;
        }
    }
}
