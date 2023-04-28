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

        [Name("AGL (ft)")]
        public int Altitude { get; set; }

        [Name("Air Speed (kts)")]
        public double AirSpeedInd { get; set; }

        [Name("Ground Speed (kts)")]
        public double GroundSpeed { get; set; }

        /// <summary>
        /// Gets or Sets HeadWind,  + ve blowing towards tail.
        /// </summary>
        [Name("Headwind (kts)")]
        public double HeadWind { get; set; }

        /// <summary>
        /// Gets or Sets CrossWind, +ve if blowing to the right.
        /// </summary>
        [Name("Crosswind (kts)")]
        public double CrossWind { get; set; }

        /// <summary>
        /// Gets or Sets SlipAngle, +ve if crosswind is blowing to the right.
        /// </summary>
        [Name("Slip Angle (deg)")]
        public double SlipAngle { get; set; }

        /// <summary>
        /// Gets or Sets Forward SlipAngle, is equal to SlipAngle when flying down the runway center line.
        /// </summary>
        [Name("Forward Slip Angle (deg)")]
        public double ForwardSlipAngle { get; set; }

        /// <summary>
        /// Gets or Sets Side SlipAngle, is equal to SlipAngle when flying significantly away from down the runway center line.
        /// </summary>
        [Name("Side Slip Angle (deg)")]
        public double SideSlipAngle { get; set; }

        [Name("Bank Angle (deg)")]
        public double BankAngle { get; set; }

        [Name("Drift Angle (deg)")]
        public double DriftAngle { get; set; }

        [Name("Heading")]
        public int Heading { get; set; }
    }
}
