namespace LsideWPF.Common
{
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

        public int Bounces { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

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
