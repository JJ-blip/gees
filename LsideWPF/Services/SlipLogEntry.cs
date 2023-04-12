namespace LsideWPF.Services
{
    using System;
    using CsvHelper.Configuration.Attributes;

    public class SlipLogEntry
    {
        [Name("Time")]
        public DateTime Time { get; set; }

        [Name("Fpm")]
        public int Fpm { get; set; }

        [Name("Air Speed (kt)")]
        public double AirSpeedInd { get; set; }

        [Name("Ground Speed (kt)")]
        public double GroundSpeed { get; set; }

        [Name("Headwind (kt)")]
        public double HeadWind { get; set; }

        [Name("Crosswind (kt)")]
        public double CrossWind { get; set; }

        [Name("SlipAngle (deg)")]
        public double SlipAngle { get; set; }

        [Name("Bank Angle (deg)")]
        public double BankAngle { get; set; }

        [Name("Bank Angle (deg)")]
        public double DriftAngle { get; set; }
    }
}
