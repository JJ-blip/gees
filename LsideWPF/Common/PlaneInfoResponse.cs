namespace LsideWPF.Services
{
    using System.Runtime.InteropServices;
    using Octokit;

    /// <summary>
    /// This structure must match 1 : 1 with the SimService.definition list contents.
    /// </summary>
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

        // The current indicated vertical speed for the aircraft.
        public double VerticalSpeed;

        public int GearPosition;
        public bool LightLandingOn;

        // Current true heading
        public double GpsGroundTrueHeading;

        // This float represents the true heading of the runway selected by the ATC.
        public double AtcRunwayHeadingDegreesTrue;

        public int CrashFlah;

        public override string ToString()
        {
            return $"response OnGround:{this.OnGround}, AltitudeAboveGround:{this.AltitudeAboveGround}, AirspeedInd: {this.AirspeedInd} LandingRate: {this.LandingRate}";
        }
    }
}