using Serilog;
using static LsideWPF.model.Events;

namespace LsideWPF.model
{
    class TakingOffState : State
    {
        override public void Initilize()
        {
            // set up for next landing
            this._context.landingResponses.Clear();
            Log.Debug("Taking Off State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (planeInfoResponse.OnGround)
            {
                if (planeInfoResponse.GroundSpeed > Properties.Settings.Default.MaxTaxiSpeedKts)
                {
                    // still taking off (above 30kts)
                }
                else
                {
                    this._context.TransitionTo(new TaxingState());
                    return;
                }
            }
            else if (planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // now flying (above 100 ft)
                FlightEventArgs e = new FlightEventArgs
                {
                    eventType = EventType.TakeOffEvent,
                    stateMachine = new StateMachine(this._context)
                };

                this.eventHandler?.Invoke(this, e);

                this._context.TransitionTo(new FlyingState());
            }
        }
    }
}
