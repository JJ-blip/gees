using CsvHelper;
using LsideWPF.Common;
using LsideWPF.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LsideWPF.Models
{
    public class SlipLogger : ISlipLogger
    {
        private bool isEnabled = Properties.Settings.Default.SlipLoggingIsEnabled;

        private List<SlipLogEntry> log = new List<SlipLogEntry>();

        private string airport = String.Empty;
        private string plane = String.Empty;

        public void Add(PlaneInfoResponse response)
        {
            if (!isEnabled)
                return;

            if (!response.LandingGearDown)
            {
                // cancel any data so far aquired
                log = new List<SlipLogEntry>();
                return;
            }

            if (string.IsNullOrEmpty(plane) && !string.IsNullOrEmpty(response.Type))
            {
                plane = response.Type;
            }
            if (string.IsNullOrEmpty(airport) && !string.IsNullOrEmpty(response.AtcRunwayAirportName))
            {
                plane = response.AtcRunwayAirportName;
            }

            // angle between heading & ground track
            double driftAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;
            // angle between 
            double slipAngle = Math.Atan(response.CrossWind / response.HeadWind) * 180 / Math.PI;

            var logEntry = new SlipLogEntry
            {
                Time = DateTime.Now,
                Fpm = Convert.ToInt32(Math.Truncate(response.RelativeWindVelocityBodyY)),
                AirSpeedInd = Math.Round(response.AirspeedInd, 1),
                HeadWind = Math.Round(response.HeadWind, 1),
                CrossWind = Math.Round(response.CrossWind, 1),
                SlipAngle = Math.Round(slipAngle, 1),
                BankAngle = Math.Round(response.PlaneBankDegrees, 1),
                DriftAngle = Math.Round(driftAngle, 1),
            };
            log.Add(logEntry);
        }

        public void WriteLogToFile()
        {
            if (!isEnabled)
                return;

            string path = GetPath();
            if (!File.Exists(path))
            {
                try
                {
                    using (var writer = new StreamWriter(path))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(log);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"While creating Slip Logger file {path}", ex);
                }
            }
        }

        private string GetPath()
        {
            // e.g "cessna_gatwick_20230427_1321.csv"
            StringBuilder filename = new StringBuilder(plane);
            if (string.IsNullOrEmpty(airport))
            {
                filename.Append("_" + airport);
            }
            filename.Append(DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");

            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            Directory.CreateDirectory(myDocs + "\\" + dir); //create if doesn't exist
            string path = myDocs + "\\" + dir + "\\" + filename;

            return path;
        }
    }
}
