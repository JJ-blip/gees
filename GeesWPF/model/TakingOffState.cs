using System.Diagnostics;
using static GeesWPF.model.Events;

namespace GeesWPF.model
{
    class TakingOffState : State
    {
        override public void Initilize()
        {
            // set up for next landing
            this._context.landingResponses.Clear();
            Debug.WriteLine("Taking Off State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (planeInfoResponse.OnGround)
            {
                if (planeInfoResponse.GroundSpeed > 30)
                {
                    // still taking off
                }
                else
                {
                    this._context.TransitionTo(new TaxingState());
                    return;
                }
            }
            else if (planeInfoResponse.AltitudeAboveGround > 100)
            {
                FlightEventArgs e = new FlightEventArgs();
                e.eventType = EventType.TakeOffEvent;
                e.stateMachine = new StateMachine(this._context);

                this.eventHandler?.Invoke(this, e);

                this._context.TransitionTo(new FlyingState());
            }
        }
    }
}
