namespace LsideWPF.Services
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

        public DataTable GetLandingLogData()
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

        // return the most recent CVS row
        // return FlightParameters or null
        public FlightParameters GetLastLanding()
        {
            DataTable dataTable = this.GetLandingLogData();
            int mostRecent = 0;

            try
            {
                FlightParameters parameters = new FlightParameters
                {
                    // Row[0] is Time
                    Name = (string)dataTable.Rows[mostRecent][1],
                    FPM = int.Parse((string)dataTable.Rows[mostRecent][2]),
                    SlowingDistance = int.Parse((string)dataTable.Rows[mostRecent][3]),
                    Gforce = Convert.ToDouble((string)dataTable.Rows[mostRecent][4]),
                    AirSpeedInd = Convert.ToDouble((string)dataTable.Rows[mostRecent][5]),
                    GroundSpeed = Convert.ToDouble((string)dataTable.Rows[mostRecent][6]),
                    HeadWind = Convert.ToDouble((string)dataTable.Rows[mostRecent][7]),
                    CrossWind = Convert.ToDouble((string)dataTable.Rows[mostRecent][8]),
                    SlipAngle = Convert.ToDouble((string)dataTable.Rows[mostRecent][9]),
                    Bounces = int.Parse((string)dataTable.Rows[mostRecent][10]),
                    BankAngle = Convert.ToDouble((string)dataTable.Rows[mostRecent][11]),
                    AimPointOffset = int.Parse((string)dataTable.Rows[mostRecent][12]),
                    CntLineOffser = int.Parse((string)dataTable.Rows[mostRecent][13]),
                    Airport = (string)dataTable.Rows[mostRecent][14],
                    DriftAngle = Convert.ToDouble((string)dataTable.Rows[mostRecent][15]),
                };
                return parameters;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public LogEntryCollection GetLandingLogEntries()
        {
            string path = GetPath();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var options = new TypeConverterOptions { Formats = new[] { "dd/MM/yyyy HH:mm" } };

                csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                var records = csv.GetRecords<LogEntry>();
                records.OrderBy(entry => entry.Time);

                LogEntryCollection logEntries = new LogEntryCollection
                {
                    records,
                };
                return logEntries;
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
    }
}
