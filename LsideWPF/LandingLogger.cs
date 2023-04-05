using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using LsideWPF.model;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LsideWPF
{
    public class LandingLogger
    {
        public static string GetPath()
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string filename = Properties.Settings.Default.LandingFile;
            Directory.CreateDirectory(myDocs + "\\" + dir); //create if doesn't exist
            string path = myDocs + "\\" + dir + "\\" + filename;

            return path;
        }

        public string MakeLogIfEmpty()
        {
            string path = GetPath();
            if (!File.Exists(path))
            {
                try
                {
                    using (var writer = new StreamWriter(path))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(new List<LogEntry>());
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"While creating Landing Logger file {path}",ex);
                }
            }
            return path;
        }

        public void EnterLog(LogEntry newLine)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = false,
            };

            var options = new TypeConverterOptions { Formats = new[] { "dd/MM/yyyy HH:mm" } };

            string path = MakeLogIfEmpty();

            // Append to the file.
            using (var stream = File.Open(path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                List<LogEntry> newRecord = new List<LogEntry>();
                newRecord.Add(newLine);
                csv.WriteRecords(newRecord);
            }
        }

        public void LogItems(List<LogEntry> logEntryItems)
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string path = myDocs + "\\" + dir;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.DefaultExt = "*.csv";
            saveFileDialog1.Filter = "CSV documents (.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.InitialDirectory = path;

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

        // Load the Landing log from file
        public DataTable LandingLog
        {
            get
            {
                MakeLogIfEmpty();
                return GetLandingLogData();
            }
        }

        public static List<LogEntry> GetLandingLogEntries()
        {
            string path = GetPath();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var options = new TypeConverterOptions { Formats = new[] { "dd/MM/yyyy HH:mm" } };

                csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                var records = csv.GetRecords<LogEntry>();
                records.OrderBy(entry => entry.Time);

                return records.ToList();
            }
        }

        public static DataTable GetLandingLogData()
        {
            var dt = new DataTable();
            string path = GetPath();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                using (var dr = new CsvDataReader(csv))
                {
                    dt.Load(dr);
                }
            }
            dt.DefaultView.Sort = "Time desc";
            dt = dt.DefaultView.ToTable();

            DataTable clone = dt.Clone();

            for (int i = 2; i < clone.Columns.Count; i++)
            {
                clone.Columns[i].DataType = dt.Columns[i].DataType;
            }

            foreach (DataRow row in dt.Rows)
            {
                clone.ImportRow(row);
            }

            return clone;
        }
    }
}
