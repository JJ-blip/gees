namespace LsideWPF.Services
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

        [Name("Slowing Distance (ft)")]
        public int SlowingDistance { get; set; }

        [Name("Impact (G)")]
        public double Gforce { get; set; }

        [Name("Air Speed Ind (kt)")]
        public double AirSpeedInd { get; set; }

        [Name("Ground Speed (kt)")]
        public double GroundSpeed { get; set; }

        // Headwind
        [Name("Relative Wind Z (kt)")]
        public double RelativeWindZ { get; set; }

        // Crosswind
        [Name("Relative Wind X (kt)")]
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

        // Headwind = -AircraftWindZ
        [Name("AircraftWindZ (Kt)")]
        public double AircraftWindZ { get; set; }

        [Name("Crosswind (Kt)")]
        public double AircraftWindX { get; set; }
    }
}
