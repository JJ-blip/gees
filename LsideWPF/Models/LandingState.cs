namespace LsideWPF.Services
{
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using static LsideWPF.Services.Events;

    public class LandingState : State
    {
        private readonly ISlipLogger slipLogger = App.Current.Services.GetService<ISlipLogger>();

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
            this.StateMachine.Bounces = 0;
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

                double slowingDistance = this.ComputeSlowingDistance(planeInfoResponse);

                // augment statemachine with slowingDistance
                this.StateMachine.SlowingDistance = slowingDistance;

                double taxiPointLongitude = planeInfoResponse.Longitude;
                double taxiPointLatitude = planeInfoResponse.Latitude;
                Log.Debug($"Slowed to Taxi speed @ position: {taxiPointLongitude}, {taxiPointLatitude}, distance: {slowingDistance} m");

                FlightEventArgs e = new FlightEventArgs(EventType.LandingEvent, new StateMachine(this.StateMachine));

                this.StateMachine.EventPublisherHandler?.Invoke(this, e);

                this.StateMachine.TransitionTo(new TaxingState());
            }
            else if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // In the air & back above altitude threshold (100 ft)
                FlightEventArgs e = new FlightEventArgs(EventType.TouchAndGoEvent, new StateMachine(this.StateMachine));

                this.StateMachine.EventPublisherHandler?.Invoke(this, e);

                this.StateMachine.TransitionTo(new FlyingState());
            }
            else if (!planeInfoResponse.OnGround)
            {
                // we are still now in the air, but were we on the ground?
                var lastPlaneInfoResponse = this.StateMachine.Responses.ElementAt(1);
                if (lastPlaneInfoResponse.OnGround)
                {
                    Log.Debug("A Bounce");

                    // bouncing
                    this.StateMachine.Bounces++;
                }

                if (this.slipLogger != null)
                {
                    // no continue slip logging is already running
                    FlightEventArgs e = new FlightEventArgs(EventType.SlipLoggingEvent, planeInfoResponse);
                    this.StateMachine.EventPublisherHandler?.Invoke(this, e);
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

        private double ComputeSlowingDistance(PlaneInfoResponse planeInfoResponse)
        {
            bool onRunway = false;

            // from aimpoint (- is short)
            double taxiPointZ = 0;
            if (planeInfoResponse.OnAnyRunway)
            {
                // taxi point data is valid
                onRunway = true;
                taxiPointZ = planeInfoResponse.AtcRunwayTdpointRelativePositionZ;
            }

            double slowingDistance;
            if (this.landedOnRunway && onRunway)
            {
                slowingDistance = taxiPointZ - this.touchdownRunwayZ;
            }
            else
            {
                // from geography - fallback
                slowingDistance = StateUtil.GetDistance(this.touchDownLongitude, this.touchDownLatitude, planeInfoResponse.Longitude, planeInfoResponse.Latitude);
            }

            return slowingDistance;
        }
    }
}
