using Serilog;
using System.Linq;
using static GeesWPF.model.Events;

namespace GeesWPF.model
{
    class LandingState : State
    {
        private double touchDownLatitude;
        private double touchDownLongitude;
        private bool touchedDown = false;

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
                touchDownLongitude = planeInfoResponse.Longitude;
                touchDownLatitude = planeInfoResponse.Latitude;
                touchedDown = true;

                Log.Debug($"Touched Ground @ {touchDownLongitude.ToString()}, {touchDownLatitude.ToString()}");    
            }

            if (planeInfoResponse.OnGround && planeInfoResponse.GroundSpeed <= Properties.Settings.Default.MaxTaxiSpeedKts)
            {
                // On the ground & below max taxi speed (30 kts)

                var taxiLongitude = planeInfoResponse.Longitude;
                var taxiLatitude = planeInfoResponse.Latitude;

                double landingDistance = StateUtil.GetDistance(touchDownLongitude, touchDownLatitude, planeInfoResponse.Longitude, planeInfoResponse.Latitude);
                this._context.landingDistance = landingDistance;

                Log.Debug($"Slowed to Taxi speed @ {taxiLongitude}, {taxiLatitude}, distance: {landingDistance} m");

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
            }
        }
    }
}
