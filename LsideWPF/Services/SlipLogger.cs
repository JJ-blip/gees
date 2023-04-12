namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using CsvHelper;
    using Serilog;

    public class SlipLogger : ISlipLogger
    {
        private readonly bool isEnabled = Properties.Settings.Default.SlipLoggingIsEnabled;

        private string airport;
        private string plane;

        private List<SlipLogEntry> log = new List<SlipLogEntry>();

        public void Add(PlaneInfoResponse response)
        {
            if (!this.isEnabled)
            {
                return;
            }

            if (!response.LandingGearDown)
            {
                // cancel any data so far aquired
                this.log = new List<SlipLogEntry>();
                return;
            }

            if (string.IsNullOrEmpty(this.plane) && !string.IsNullOrEmpty(response.Type))
            {
                this.plane = response.Type;
            }

            if (string.IsNullOrEmpty(this.airport) && !string.IsNullOrEmpty(response.AtcRunwayAirportName))
            {
                this.airport = response.AtcRunwayAirportName;
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
            this.log.Add(logEntry);
        }

        public void Reset()
        {
            // TODO
            throw new NotImplementedException();
        }

        public void WriteLogToFile()
        {
            if (!this.isEnabled)
            {
                return;
            }

            string path = this.GetPath();
            if (!File.Exists(path))
            {
                try
                {
                    using (var writer = new StreamWriter(path))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(this.log);
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
            StringBuilder filename = new StringBuilder(this.plane);
            if (string.IsNullOrEmpty(this.airport))
            {
                filename.Append("_" + this.airport);
            }

            filename.Append(DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");

            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;

            // create if doesn't exist
            Directory.CreateDirectory(myDocs + "\\" + dir);
            string path = myDocs + "\\" + dir + "\\" + filename;

            return path;
        }
    }
}
