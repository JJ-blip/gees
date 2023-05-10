namespace LsideWPF.Common
{
    using System;
    using System.Runtime.InteropServices;

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

        // RelativeWindVelocityBodyX (AirSpeed)
        public double RelativeWindX;

        // RelativeWindVelocityBodyZ (Airspeed)
        public double RelativeWindZ;

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

        // AtcRunwayTdpointRelativePositionX Ft
        // Right (+) or left (-) of the runway centerline
        public double AtcRunwayTdpointRelativePositionX;

        // AtcRunwayTdpointRelativePositionZ Ft
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

        // -Headwind (Windspeed)
        public double AircraftWindZ;

        // Crosswind (windspeed)
        public double AircraftWindX;

        // z position on runway Ft
        public double AtcRunwayRelativePositionZ;

        // incrementing Id, always place as last field.
        public long Id;

        public override string ToString()
        {
            return $"response Id:{this.Id}, OnGround:{this.OnGround}, AltitudeAboveGround:{this.AltitudeAboveGround}, AirspeedInd: {this.AirspeedInd} LandingRate: {this.LandingRate}";
        }
    }
}