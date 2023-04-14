namespace LsideWPF.Services
{
    /// <summary>
    /// A mixture of data from one Simulation data packet and number of computed parameters.
    /// Typically from data at captured at touchdown, augmented buy data at commencement of taxing.
    /// </summary>
    public class FlightParameters
    {
        public string Name { get; set; }

        public int FPM { get; set; }

        public double Gforce { get; set; }

        public double AirSpeedInd { get; set; }

        public double GroundSpeed { get; set; }

        public double HeadWind { get; set; }

        public double SlipAngle { get; set; }

        public double CrossWind { get; set; }

        // aggregrated count over a landing
        public int Bounces { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        // computed from data within Simulation data packet at point taxing commenced
        public int SlowingDistance { get; set; }

        public double BankAngle { get; set; }

        public string Airport { get; set; }

        public int AimPointOffset { get; set; }

        public int CntLineOffser { get; set; }

        public double RelativeWindVelocityBodyX { get; set; }

        public double RelativeWindVelocityBodyZ { get; set; }

        public double DriftAngle { get; set; }
    }
}
