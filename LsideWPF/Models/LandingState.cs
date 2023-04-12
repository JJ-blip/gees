namespace LsideWPF.Models
{
    using System.Linq;
    using LsideWPF.Common;
    using Serilog;
    using static LsideWPF.Models.Events;

    public class LandingState : State
    {
        private bool touchedDown = false;
        private bool landed = false;
        private double touchDownLatitude;
        private double touchDownLongitude;

        private bool landedOnRunway = false;
        private string airport;
        private double touchDownRunwayX;
        private double touchdownRunwayZ;

        public LandingState()
        {
        }

        public override void Initilize()
        {
            this.stateMachine.Bounces = 0;
            Log.Debug("Landing State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            // Log.Debug($"Landing: {idx++}, {planeInfoResponse.ToString()}");
            if (planeInfoResponse.OnGround && !this.touchedDown)
            {
                // touch down & speed will be high
                this.touchedDown = true;
                this.touchDownLongitude = planeInfoResponse.Longitude;
                this.touchDownLatitude = planeInfoResponse.Latitude;

                if (planeInfoResponse.OnAnyRunway)
                {
                    // touchdown data is valid
                    this.landedOnRunway = true;
                    this.airport = planeInfoResponse.AtcRunwayAirportName;
                    this.touchDownRunwayX = planeInfoResponse.AtcRunwayTdpointRelativePositionX;
                    this.touchdownRunwayZ = planeInfoResponse.AtcRunwayTdpointRelativePositionZ;
                }

                Log.Debug($"Touched Ground @ {this.airport} {this.touchDownLongitude}, {this.touchDownLatitude}");
            }

            if (!this.landed && planeInfoResponse.OnGround && planeInfoResponse.GroundSpeed <= Properties.Settings.Default.MaxTaxiSpeedKts)
            {
                // On the ground & below max taxi speed (30 kts)
                this.landed = true;
                var taxiPointLongitude = planeInfoResponse.Longitude;
                var taxiPointLatitude = planeInfoResponse.Latitude;

                bool onRunway = false;

                // from centerline
                double taxiPointX = 0;

                // from aimpoint (- is short)
                double taxiPointZ = 0;
                if (planeInfoResponse.OnAnyRunway)
                {
                    // taxi point data is valid
                    onRunway = true;
                    taxiPointX = planeInfoResponse.AtcRunwayTdpointRelativePositionX;
                    taxiPointZ = planeInfoResponse.AtcRunwayTdpointRelativePositionZ;
                }

                double slowingDistance;
                if (this.landedOnRunway && onRunway)
                {
                    slowingDistance = taxiPointZ - this.touchdownRunwayZ;
                }
                else
                {
                    slowingDistance = StateUtil.GetDistance(this.touchDownLongitude, this.touchDownLatitude, planeInfoResponse.Longitude, planeInfoResponse.Latitude);
                }

                this.stateMachine.SlowingDistance = slowingDistance;

                Log.Debug($"Slowed to Taxi speed @ position: {taxiPointLongitude}, {taxiPointLatitude}, distance: {slowingDistance} m");

                FlightEventArgs e = new FlightEventArgs
                {
                    EventType = EventType.LandingEvent,
                    StateMachine = new StateMachine(this.stateMachine),
                };

                this.eventHandler?.Invoke(this, e);

                this.stateMachine.TransitionTo(new TaxingState());
            }
            else if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // In the air & back above altitude threshold (100 ft)
                FlightEventArgs e = new FlightEventArgs
                {
                    EventType = EventType.TouchAndGoEvent,
                    StateMachine = new StateMachine(this.stateMachine),
                };

                this.eventHandler?.Invoke(this, e);

                this.stateMachine.TransitionTo(new FlyingState());
            }
            else if (!planeInfoResponse.OnGround)
            {
                // we are still now in the air, but were we on the ground?
                var lastPlaneInfoResponse = this.stateMachine.responses.ElementAt(1);
                if (lastPlaneInfoResponse.OnGround)
                {
                    Log.Debug("A Bounce");

                    // bouncing
                    this.stateMachine.Bounces++;
                }

                if (this.slipLogger != null)
                {
                    // no continue slip logging is already running
                    FlightEventArgs e = new FlightEventArgs
                    {
                        EventType = EventType.SlipLoggingEvent,
                        PlaneInfoResponse = planeInfoResponse,
                    };
                    this.eventHandler?.Invoke(this, e);
                }

                /*
                    //still getting down.
                    double slipAngle = Math.Atan(planeInfoResponse.CrossWind / planeInfoResponse.HeadWind) * 180 / Math.PI;
                    var msg =
                            $"Landing - AltitudeAboveGround: {planeInfoResponse.AltitudeAboveGround} "
                        + $", slipAngle: {slipAngle}";
                    Log.Debug(msg);
                */
            }
        }
    }
}
