namespace LsideWPF.Services
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Events;
    using static LsideWPF.Services.Events;

    /// <summary>
    /// On our way down, between flying and the taxing state.
    /// </summary>
    public class LandingState : State
    {
        private readonly ISlipLogger slipLogger = App.Current.Services.GetService<ISlipLogger>();

        private bool touchedDown = false;
        private bool landed = false;
        private double touchDownLatitude;
        private double touchDownLongitude;

        private bool landedOnRunway = false;
        private string airport;

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
            if (planeInfoResponse.OnGround && !this.touchedDown)
            {
                // just touched down
                this.touchedDown = true;
                this.touchDownLongitude = planeInfoResponse.Longitude;
                this.touchDownLatitude = planeInfoResponse.Latitude;

                this.slipLogger.FinishLogging();

                if (planeInfoResponse.OnAnyRunway)
                {
                    // runway data is valid
                    this.landedOnRunway = true;
                    this.airport = planeInfoResponse.AtcRunwayAirportName;
                    this.touchdownRunwayZ = planeInfoResponse.AtcRunwayTdpointRelativePositionZ;
                }

                Log.Debug($"Touched Ground @ {this.airport} {this.touchDownLongitude}, {this.touchDownLatitude}");
            }

            if (!this.landed && planeInfoResponse.OnGround && planeInfoResponse.GroundSpeed <= Properties.Settings.Default.MaxTaxiSpeedKts)
            {
                // just landed and speed dropped below 'max taxi speed' (30 kts)
                this.landed = true;

                // augment statemachine with slowingDistance
                double slowingDistance = this.ComputeSlowingDistance(planeInfoResponse);
                this.StateMachine.SlowingDistance = slowingDistance;

                if (Log.IsEnabled(LogEventLevel.Debug))
                {
                    double taxiPointLongitude = planeInfoResponse.Longitude;
                    double taxiPointLatitude = planeInfoResponse.Latitude;
                    Log.Debug($"Slowed to Taxi speed @ position: {taxiPointLongitude}, {taxiPointLatitude}, distance: {slowingDistance} m");
                }

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

                this.slipLogger.CancelLogging();
            }
            else if (!planeInfoResponse.OnGround)
            {
                // we are still now in the air, but were we on the ground?
                var previousAddedIdx = Math.Max (0, this.StateMachine.Responses.Count() - 2);
                var lastPlaneInfoResponse = this.StateMachine.Responses.ElementAt(previousAddedIdx);
                if (lastPlaneInfoResponse.OnGround)
                {
                    Log.Debug("A Bounce");

                    // bouncing
                    this.StateMachine.Bounces++;
                }

                this.slipLogger.Log(planeInfoResponse);
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
