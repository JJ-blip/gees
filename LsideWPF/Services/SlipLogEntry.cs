﻿namespace LsideWPF.Services
{
    using System;
    using CsvHelper.Configuration.Attributes;

    public class SlipLogEntry
    {
        // Order of decleration sets colomn order in CSV file.

        // Local Time of sample
        [Name("Time")]
        public DateTime Time { get; set; }

        [Name("AGL (ft)")]
        public int Altitude { get; set; }

        [Name("Vertical Speed (fpm)")]
        public int VerticalSpeed { get; set; }

        [Name("Air Speed (kts)")]
        public double AirSpeedInd { get; set; }

        [Name("Ground Speed (kts)")]
        public double GroundSpeed { get; set; }

        /// <summary>
        /// Gets or Sets RelativeWindZ,  + ve blowing towards tail.
        /// </summary>
        [Name("RelativeWindZ (kts)")]
        public double RelativeWindZ { get; set; }

        /// <summary>
        /// Gets or Sets RelativeWindX, +ve if blowing to the right.
        /// </summary>
        [Name("RelativeWindX (kts)")]
        public double RelativeWindX { get; set; }

        /// <summary>
        /// Gets or Sets SlipAngle, +ve if crosswind is blowing to the right.
        /// </summary>
        [Name("Slip Angle (deg)")]
        public double SlipAngle { get; set; }

        [Name("Bank Angle (deg)")]
        public double BankAngle { get; set; }

        [Name("Drift Angle (deg)")]
        public double DriftAngle { get; set; }

        [Name("Heading")]
        public int Heading { get; set; }

        // - AircraftWindZ
        [Name("Headwind (kts)")]
        public double Headwind { get; set; }

        // + AircraftWindX
        [Name("Crosswind (Kts)")]
        public double Crosswind { get; set; }

        public long Id { get; set; }
    }
}
