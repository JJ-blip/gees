using System.Runtime.InteropServices;

namespace LsideWPF.Common
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneInfoResponse
    {
        // Title
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Type;

        // SimOnGround
        public bool OnGround;

        // AircraftWindX
        public double CrossWind;

        // AircraftWindz
        public double HeadWind;

        // AirspeedIndicated
        public double AirspeedInd;

        // GroundVelocity
        public double GroundSpeed;

        // VelocityBodyX
        public double LateralSpeed;

        // VelocityBodyZ
        public double SpeedAlongHeading;

        // Gforce
        public double Gforce;

        // PlaneTouchdownNormalVelocity
        public double LandingRate;

        // PlaneAltitudeAboveGround
        public double AltitudeAboveGround;

        // PlaneLatitude
        public double Latitude;

        // PlaneLongitude
        public double Longitude;

        // PlaneBankDegrees
        public double PlaneBankDegrees;

        // OnAnyRunway
        public bool OnAnyRunway;

        // AtcRunwayAirportName
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string AtcRunwayAirportName;

        // AtcRunwaySelected
        public bool AtcRunwaySelected;

        // AtcRunwayTdpointRelativePositionX
        // Right (+) or left (-) of the runway centerline
        public double AtcRunwayTdpointRelativePositionX;

        // AtcRunwayTdpointRelativePositionZ
        // Forward (+) or backward (-) of the runway aimingpoint (2 wide markers, beyond threshold)
        public double AtcRunwayTdpointRelativePositionZ;

        // RelativeWindVelocityBodyX
        // Sideways - Lateral speed relative to Wind
        public double RelativeWindVelocityBodyX;

        // RelativeWindVelocityBodyY
        // Vertical speed relative to Wind
        public double RelativeWindVelocityBodyY;

        // RelativeWindVelocityBodyZ
        // Longitudal Speed relative to Wind
        public double RelativeWindVelocityBodyZ;

        public bool LandingGearDown;

        // AmbientWindX (E - W)
        public double AmbientWindX;

        // AmbientWindZ (N - S)
        public double AmbientWindZ;

        public override string ToString()
        {
            return $"response OnGround:{OnGround}, AltitudeAboveGround:{AltitudeAboveGround}, AirspeedInd: {AirspeedInd} LandingRate: {LandingRate}";
        }
    }
}