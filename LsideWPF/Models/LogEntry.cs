namespace LsideWPF.Models
{
    using System;
    using CsvHelper.Configuration.Attributes;

    // Landing data
    public class LogEntry
    {
        [Name("Time")]
        public DateTime Time { get; set; }

        [Name("Plane")]
        public string Plane { get; set; }

        [Name("FPM")]
        public int Fpm { get; set; }

        // Specifically Roll distance to achieving Max Taxi speed
        [Name("Slowing Distance (ft)")]
        public int SlowingDistance { get; set; }

        [Name("Impact (G)")]
        public double Gforce { get; set; }

        [Name("Air Speed Ind (kts)")]
        public double AirSpeedInd { get; set; }

        [Name("Ground Speed (kts)")]
        public double GroundSpeed { get; set; }

        // - Relative Headwind
        [Name("Relative Wind Z (kts)")]
        public double RelativeWindZ { get; set; }

        // Crosswind
        [Name("Relative Wind X (kts)")]
        public double RelativeWindX { get; set; }

        [Name("SlipAngle (deg)")]
        public double SlipAngle { get; set; }

        [Name("Bounces")]
        public int Bounces { get; set; }

        [Name("Bank Angle (deg)")]
        public double BankAngle { get; set; }

        [Name("Distance From Aim Point (ft)")]
        public int AimPointOffset { get; set; }

        [Name("Offser From Cnt Line (ft)")]
        public int CntLineOffser { get; set; }

        [Name("Airport")]
        public string Airport { get; set; }

        [Name("DriftAngle (deg)")]
        public double DriftAngle { get; set; }

        [Name("AircraftWindZ (kts)")]
        public double AircraftWindZ { get; set; }

        [Name("AircraftWindX (kts)")]
        public double AircraftWindX { get; set; }

        [Name("Headwind (kts)")]
        public double AverageHeadwind { get; set; }

        [Name("Crosswind (kts)")]
        public double AverageCrosswind { get; set; }

        [Ignore]
        public double Latitude { get; set; }

        [Ignore]
        public double Longitude { get; set; }

        [Name("Landed On Runway")]
        public bool LandedOnRunway { get; set; }

        // Only meaningfull if entry derives 'directly' from a planeInfoResponse
        [Ignore]
        public long Id { get; set; }

        public override string ToString()
        {
            return $"logEntry Time:{this.Time}, FPM:{this.Fpm},";
        }
    }
}
