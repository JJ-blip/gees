using System.Diagnostics;
using System.Linq;
using static GeesWPF.model.Events;

namespace GeesWPF.model
{
    class LandingState : State
    {
        private double touchDownLatitude;
        private double touchDownLongitude;

        public LandingState()
        {
        }

        override public void Initilize()
        {
            this._context.Bounces = 0;
            Debug.WriteLine("Landing State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            var lastPlaneInfoResponse = _context.responses.ElementAt(1);
            if (!lastPlaneInfoResponse.OnGround)
            {
                if (planeInfoResponse.OnGround)
                {
                    touchDownLatitude = planeInfoResponse.Latitude;
                    touchDownLongitude = planeInfoResponse.Longitude;

                    Debug.WriteLine("Touched Ground");
                }
            }

            if (planeInfoResponse.OnGround)
            {

                if (!(planeInfoResponse.GroundSpeed > 30))
                {
                    Debug.WriteLine("Slowing, at Taxi speed");

                    double landingDistance = StateUtil.GetDistance(touchDownLongitude, touchDownLatitude, planeInfoResponse.Longitude, planeInfoResponse.Latitude);
                    this._context.landingDistance = landingDistance;

                    FlightEventArgs e = new FlightEventArgs();
                    e.eventType = EventType.LandingEvent;
                    e.stateMachine = new StateMachine(this._context);

                    this.eventHandler?.Invoke(this, e);

                    this._context.TransitionTo(new TaxingState());
                }
                else
                {
                    // still landing
                    return;
                }
            }
            else if (planeInfoResponse.AltitudeAboveGround > 100)
            {
                FlightEventArgs e = new FlightEventArgs();
                e.eventType = EventType.TouchAndGoEvent;
                e.stateMachine = new StateMachine(this._context);

                this.eventHandler?.Invoke(this, e);

                this._context.TransitionTo(new FlyingState());
            }
            else
            {
                // is InAir

                lastPlaneInfoResponse = _context.responses.ElementAt(1);
                if (lastPlaneInfoResponse.OnGround)
                {
                    Debug.WriteLine("A Bounce");
                    // bouncing
                    this._context.Bounces++;
                }
                else
                {
                    // still landing
                    return;
                }
            }
        }
    }
}
