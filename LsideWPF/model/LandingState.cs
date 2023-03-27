using Microsoft.FlightSimulator.SimConnect;
using Serilog;
using System.Linq;
using static LsideWPF.model.Events;

namespace LsideWPF.model
{
    class LandingState : State
    {
        private bool touchedDown = false;
        private double touchDownLatitude;
        private double touchDownLongitude;

        private bool onAtcControlledRunway = false;
        private string airport;
        private double touchDownRunwayX;
        private double touchdownRunwayZ;
        // just a progress indicator
        private int idx = 0;

        public LandingState()
        {            
        }

        override public void Initilize()
        {
            this._context.Bounces = 0;
            Log.Debug("Landing State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            Log.Debug($"Landing: {idx++}, {planeInfoResponse.ToString()}");

            if (planeInfoResponse.OnGround && !touchedDown)
            {
                // touch down & speed will be high
                touchedDown = true;
                touchDownLongitude = planeInfoResponse.Longitude;
                touchDownLatitude = planeInfoResponse.Latitude;

                if (planeInfoResponse.AtcRunwaySelected && planeInfoResponse.OnAnyRunway)
                {
                    onAtcControlledRunway = true;
                    airport = planeInfoResponse.AtcRunwayAirportName;
                    touchDownRunwayX = planeInfoResponse.AtcRunwayTdpointRelativePositionX;
                    touchdownRunwayZ = planeInfoResponse.AtcRunwayTdpointRelativePositionZ;
                }

                Log.Debug($"Touched Ground @ {airport} {touchDownLongitude.ToString()}, {touchDownLatitude.ToString()}");    
            }

            if (planeInfoResponse.OnGround && planeInfoResponse.GroundSpeed <= Properties.Settings.Default.MaxTaxiSpeedKts)
            {
                // On the ground & below max taxi speed (30 kts)

                var taxiPointLongitude = planeInfoResponse.Longitude;
                var taxiPointLatitude = planeInfoResponse.Latitude;

                bool onAtcRunway = false;
                // from centerline
                double taxiPointX  =0;
                // from aimpoint (- is short)
                double taxiPointZ = 0; 
                if (planeInfoResponse.AtcRunwaySelected && planeInfoResponse.OnAnyRunway)
                {
                    onAtcRunway = true;
                    taxiPointX = planeInfoResponse.AtcRunwayTdpointRelativePositionX;
                    taxiPointZ = planeInfoResponse.AtcRunwayTdpointRelativePositionZ;
                }

                double slowingDistance;
                if (onAtcRunway)
                {
                    slowingDistance = taxiPointZ - touchdownRunwayZ;
                }
                else
                {
                    slowingDistance = StateUtil.GetDistance(touchDownLongitude, touchDownLatitude, planeInfoResponse.Longitude, planeInfoResponse.Latitude); 
                }

                this._context.SlowingDistance = slowingDistance;

                Log.Debug($"Slowed to Taxi speed @ {taxiPointLongitude}, {taxiPointLatitude}, distance: {slowingDistance} m");

                FlightEventArgs e = new FlightEventArgs
                {
                    eventType = EventType.LandingEvent,
                    stateMachine = new StateMachine(this._context)
                };

                this.eventHandler?.Invoke(this, e);

                this._context.TransitionTo(new TaxingState());
            }
            else if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // In the air & back above altitude threshold (100 ft)

                FlightEventArgs e = new FlightEventArgs
                {
                    eventType = EventType.TouchAndGoEvent,
                    stateMachine = new StateMachine(this._context)
                };

                this.eventHandler?.Invoke(this, e);

                this._context.TransitionTo(new FlyingState());
            }
            else if (!planeInfoResponse.OnGround)
            {
                // now in the air, but were we on the ground 

                var lastPlaneInfoResponse = _context.responses.ElementAt(1);
                if (lastPlaneInfoResponse.OnGround)
                {
                    Log.Debug("A Bounce");
                    // bouncing
                    this._context.Bounces++;
                }

                /*
                // ATC log data
                var msg = 
                      $"PlaneBankDegrees, {planeInfoResponse.PlaneBankDegrees}, " 
                    + $"OnAnyRunway, {planeInfoResponse.OnAnyRunway}, "
                    + $"AtcRunwayAirportName, {planeInfoResponse.AtcRunwayAirportName}, "
                    + $"AtcRunwaySelected, {planeInfoResponse.AtcRunwaySelected} "
                //    + $"AtcRunwayRelativePositionX, {planeInfoResponse.AtcRunwayRelativePositionX} "
                //    + $"AtcRunwayRelativePositionZ, {planeInfoResponse.AtcRunwayRelativePositionZ} "
                    + $"AtcRunwayTdpointRelativePositionX, {planeInfoResponse.AtcRunwayTdpointRelativePositionX} "
                //    + $"AtcRunwayTdpointRelativePositionY, {planeInfoResponse.AtcRunwayTdpointRelativePositionY} "
                    + $"AtcRunwayTdpointRelativePositionZ, {planeInfoResponse.AtcRunwayTdpointRelativePositionZ}";
                Log.Debug(msg);
                */
            }
        }
    }
}
