namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using LsideWPF.Common;
    using LsideWPF.Utils;

    public class SlipLogger : ISlipLogger, INotifyPropertyChanged
    {
        // 600 @ half seconds = about 5 mins final approach
        private const int QueueSize = 600;
        private const int AtMostFrequencyMSec = 500;

        private const string TimeFormat = "dd/MM/yyyy HH:mm:ss.fff";

        private readonly bool isEnabled = Properties.Settings.Default.SlipLoggingIsEnabled;

        private string airport;
        private string plane;
        private DateTime lastEntry;
        private bool isArmed = false;

        private string savedToFilename = string.Empty;
        private bool acquisitionComplete = false;

        // use queue to protect overflow
        private BoundedQueue<SlipLogEntry> log = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<SlipLogEntry> GetLogEntries()
        {
            if (!this.acquisitionComplete)
            {
                return new List<SlipLogEntry>();
            }

            var en = this.log.GetEnumerator();

            var result = new List<SlipLogEntry>();
            while (en.MoveNext())
            {
                result.Add((SlipLogEntry)en.Current);
            }

            return result;
        }

        public string GetFullFilename()
        {
            if (!this.acquisitionComplete)
            {
                return $"No Data available yet";
            }

            return this.savedToFilename;
        }

        public bool HasCompleted()
        {
            return this.acquisitionComplete;
        }

        public bool IsArmed()
        {
            return this.isArmed;
        }

        public void Log(PlaneInfoResponse response)
        {
            if (!this.isEnabled || !this.isArmed)
            {
                return;
            }

            if (!response.OnGround && (this.lastEntry != null && (DateTime.Now - this.lastEntry).Milliseconds <= AtMostFrequencyMSec))
            {
                // log atmost every 500 milli seconds except if is touchdown entry
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

            var logEntry = this.GetSlipLogEntry(response);

            this.log.Enqueue(logEntry);
            this.lastEntry = logEntry.Time;
        }

        public void BeginLogging()
        {
            if (!this.isArmed)
            {
                // bound queue against stupid sizes
                this.log = new BoundedQueue<SlipLogEntry>(QueueSize);
                this.isArmed = true;
                this.acquisitionComplete = false;
                this.PropertyChanged(this, new PropertyChangedEventArgs("HasCompleted"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsArmed"));
            }
        }

        public void FinishLogging()
        {
            if (!this.isEnabled || !this.isArmed)
            {
                return;
            }

            string path = this.GetPath();
            if (!File.Exists(path))
            {
                try
                {
                    CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture);

                    using (var writer = new StreamWriter(path))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        var options = new TypeConverterOptions { Formats = new[] { TimeFormat } };
                        csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);

                        csv.WriteRecords(this.log);
                    }

                    this.savedToFilename = path;
                    this.acquisitionComplete = true;
                    this.PropertyChanged(this, new PropertyChangedEventArgs("HasCompleted"));
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"While creating Slip Logger file {path}", ex);
                }
            }

            this.isArmed = false;
            this.PropertyChanged(this, new PropertyChangedEventArgs("IsArmed"));
        }

        public List<SlipLogEntry> GetListDataTable(DataTable dt)
        {
            List<SlipLogEntry> list = new List<SlipLogEntry>();

            foreach (DataRow row in dt.Rows)
            {
                SlipLogEntry logEntry = new SlipLogEntry
                {
                    Time = DateTime.ParseExact((string)row[0], TimeFormat, CultureInfo.InvariantCulture),
                    Altitude = Convert.ToInt32(row[1]),
                    VerticalSpeed = Convert.ToInt32(row[2]),
                    GroundSpeed = Convert.ToDouble(row[3]),
                    AirSpeedInd = Convert.ToDouble(row[4]),
                    RelativeWindZ = Convert.ToDouble(row[5]),
                    RelativeWindX = Convert.ToDouble(row[6]),
                    SlipAngle = Convert.ToDouble(row[7]),
                    BankAngle = Convert.ToDouble(row[8]),
                    DriftAngle = Convert.ToDouble(row[9]),
                    Heading = Convert.ToInt32(row[10]),
                    Crosswind = Convert.ToDouble(row[11]),
                    Headwind = Convert.ToDouble(row[12]),
                };

                list.Add(logEntry);
            }

            List<SlipLogEntry> sortedList = list.OrderBy(logEntry => logEntry.Time).ToList();

            return sortedList;
        }

        public void CancelLogging()
        {
            this.acquisitionComplete = false;
            this.isArmed = false;
            this.log = null;

            this.PropertyChanged(this, new PropertyChangedEventArgs("HasCompleted"));
            this.PropertyChanged(this, new PropertyChangedEventArgs("IsArmed"));
        }

        // computes an average of the Slip results, if the slip collection relates to the requiredId
        public double GetAverageHeadwind(long requiredId)
        {
            if (this.log == null)
            {
                // no slip data available
                return double.NaN;
            }

            LinkedList<SlipLogEntry> entries = this.log.GetInternalLinkList();

            if (this.acquisitionComplete && entries.Select(item => item.Id).Contains(requiredId))
            {
                // guard is satisfied
                var avg = entries.Average(item => item.Headwind);
                return avg;
            }
            else
            {
                return double.NaN;
            }
        }

        // computes an average of the Slip results, if the slip collection relates to the requiredId
        public double GetAverageCrosswind(long requiredId)
        {
            if (this.log == null)
            {
                return double.NaN;
            }

            LinkedList<SlipLogEntry> entries = this.log.GetInternalLinkList();

            if (this.acquisitionComplete && entries.Select(item => item.Id).Contains(requiredId))
            {
                // guard is satisfied
                var avg = entries.Average(item => item.Crosswind);
                return avg;
            }
            else
            {
                return double.NaN;
            }
        }

        private string GetPath()
        {
            // e.g "cessna_gatwick_20230427_1321.csv"
            StringBuilder filename = new StringBuilder($"SlipLog-{this.plane}");
            if (!string.IsNullOrEmpty(this.airport))
            {
                filename.Append($"-{this.airport}");
            }

            filename.Append($"-{DateTime.Now:yyyyMMdd_HHmm}.csv");

            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;

            // create if doesn't exist
            Directory.CreateDirectory($"{myDocs}\\{dir}");
            string path = $"{myDocs}\\{dir}\\{filename}";

            return path;
        }

        private SlipLogEntry GetSlipLogEntry(PlaneInfoResponse response)
        {
            // angle between heading & ground track
            double driftAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;

            // angle between
            double slipAngle = this.GetSlipAngle(response);

            var logEntry = new SlipLogEntry
            {
                Id = response.Id,
                Time = DateTime.Now,
                Altitude = Convert.ToInt32(Math.Truncate(response.AltitudeAboveGround)),
                VerticalSpeed = Convert.ToInt32(Math.Truncate(response.VerticalSpeed)),
                GroundSpeed = Math.Round(response.GroundSpeed, 0),
                AirSpeedInd = Math.Round(response.AirspeedInd, 0),
                RelativeWindZ = Math.Round(response.RelativeWindZ, 1),
                RelativeWindX = Math.Round(response.RelativeWindX, 1),
                SlipAngle = Math.Round(slipAngle, 1),
                BankAngle = Math.Round(response.PlaneBankDegrees, 1),
                DriftAngle = Math.Round(driftAngle, 1),
                Heading = Convert.ToInt32(Math.Truncate(response.GpsGroundTrueHeading)),
                Crosswind = Math.Round(response.AircraftWindX, 1),
                Headwind = -Math.Round(response.AircraftWindZ, 1),
            };

            return logEntry;
        }

        private double GetSlipAngle(PlaneInfoResponse response)
        {
            // Angle between RelativeWindVelocityX & RelativeWindVelocityZ (I.e the 2 Airspeeds)
            // By convention
            // A Headwi
            return Math.Atan(response.RelativeWindX / response.RelativeWindZ) * 180 / Math.PI;
        }

        /// <summary>
        /// Gets Center Line Offset Degrees
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
        ///
        /// The thinking behind this logic is flawed and thus not used.
        /// Its been left pending further consideration.
        ///
        /// compute the slip components.
        /// 'Slip' defined as angle between Relative Wind and Planes Heading (Z axis)
        /// +ve if wind blows on Right side of fuselarge.
        ///
        /// In the absence of a ground reference all Slip is 'SideSlip'
        /// In presence of a ground reference, specifically in this code, Runway centerline.
        ///
        /// If heading is down the Runway centerline and Relative Wind is blowing directly on the nose
        /// Then all the slip is 'Forward Slip'.
        ///
        /// If heading is offset from the runway centerline or Relative Wind is not on the nose
        /// This code arbitarily approttions the slip as follows.
        ///
        ///   1. Slip = Forward Slip + Side Slip.
        ///
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
    }
}
