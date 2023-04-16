namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Text;
    using CsvHelper;
    using LsideWPF.Utils;
    using Serilog;

    public class SlipLogger : ISlipLogger
    {
        // 200 @ 2 seconds = about 6 mins final approach
        private const int QueueSize = 200;
        private const int AtMostFrequency = 2;

        private readonly bool isEnabled = Properties.Settings.Default.SlipLoggingIsEnabled;

        private string airport;
        private string plane;
        private DateTime lastEntry;
        private bool isArmed = false;

        // use queue to protect overflow
        private BoundedQueue<SlipLogEntry> log = null;

        public void Log(PlaneInfoResponse response)
        {
            if (!this.isEnabled || !this.isArmed)
            {
                return;
            }

            if (this.lastEntry != null && (DateTime.Now - this.lastEntry).Seconds < AtMostFrequency)
            {
                // log atmost every 2 seconds
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
            double slipAngle = this.GetSlipAngle(response);

            var (sideSlip, forwardSlip) = this.GetSlipComponents(response);

            var logEntry = new SlipLogEntry
            {
                Time = DateTime.Now,
                Fpm = Convert.ToInt32(Math.Truncate(response.VerticalSpeed)),
                AirSpeedInd = Math.Round(response.AirspeedInd, 1),
                HeadWind = Math.Round(response.HeadWind, 1),
                CrossWind = Math.Round(response.CrossWind, 1),
                SlipAngle = Math.Round(slipAngle, 1),
                BankAngle = Math.Round(response.PlaneBankDegrees, 1),
                DriftAngle = Math.Round(driftAngle, 1),
                SideSlipAngle = Math.Round(sideSlip, 1),
                ForwardSlipAngle = Math.Round(forwardSlip, 1),
                CrashFlag = this.GetCrashDetail(response.CrashFlah),
            };
            this.log.Enqueue(logEntry);
            this.lastEntry = logEntry.Time;
        }

        public void BeginLogging()
        {
            // bound queue against stupid sizes
            this.log = new BoundedQueue<SlipLogEntry>(QueueSize);
            this.isArmed = true;
        }

        public void FinishLogging()
        {
            if (!this.isEnabled || !this.isArmed)
            {
                return;
            }

            this.isArmed = false;

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
                    Serilog.Log.Error($"While creating Slip Logger file {path}", ex);
                }
            }
        }

        public void CancelLogging()
        {
            this.isArmed = false;
            this.log = null;
        }

        private string GetPath()
        {
            // e.g "cessna_gatwick_20230427_1321.csv"
            StringBuilder filename = new StringBuilder($"SlipLog-{this.plane}");
            if (string.IsNullOrEmpty(this.airport))
            {
                filename.Append($"- {this.airport}");
            }

            filename.Append($"{DateTime.Now:yyyyMMdd_HHmm}.csv");

            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;

            // create if doesn't exist
            Directory.CreateDirectory($"{myDocs}\\{dir}");
            string path = $"{myDocs}\\{dir}\\{filename}";

            return path;
        }

        private double GetSlipAngle(PlaneInfoResponse response)
        {
            return Math.Atan(response.CrossWind / response.HeadWind) * 180 / Math.PI;
        }

        /// <summary>
        /// Gets or Sets Center Line Offset Degrees
        /// being difference of GPS GROUND TRUE HEADING minus ATC RUNWAY HEADING DEGREES TRUE.
        /// </summary>
        private double GetCenterLineOffsetDegrees(PlaneInfoResponse response)
        {
            if (response.AtcRunwaySelected)
            {
                var diff = response.GpsGroundTrueHeading - response.AtcRunwayHeadingDegreesTrue;
                if (diff > 180)
                {
                    diff -= 360;
                }
                else if (diff < -180)
                {
                    diff += 360;
                }

                return diff;
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// compute the slip components.
        /// </summary>
        /// <returns>Tuple (sideSlip: ForwardSlip:).</returns>
        private (double, double) GetSlipComponents(PlaneInfoResponse response)
        {
            double slip = this.GetSlipAngle(response);
            if (!response.AtcRunwaySelected)
            {
                return (this.GetSlipAngle(response), 0.0);
            }
            else
            {
                double r = this.GetCenterLineOffsetDegrees(response);
                double fs;
                if (slip >= 0)
                {
                    fs = Math.Max(0, slip - Math.Abs(r));
                }
                else
                {
                    fs = Math.Min(0, slip + Math.Abs(r));
                }

                double ss = slip - fs;
                return (ss, fs);
            }
        }

        private string GetCrashDetail(int crashFlag)
        {
            switch (crashFlag)
            {
                case 0:
                    return string.Empty;
                case 2:
                    return "Mountains";
                case 4:
                    return "General";
                case 6:
                    return "Building";
                case 8:
                    return "Splash";
                case 10:
                    return "Gear up";
                case 12:
                    return "Overstress";
                case 14:
                    return "Building";
                case 16:
                    return "Aircraft";
                case 18:
                    return "Fuel Truck";
                default:
                    return string.Empty;
            }
        }
    }
}
