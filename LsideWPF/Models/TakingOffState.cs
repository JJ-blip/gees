namespace LsideWPF.Models
{
    using LsideWPF.Common;
    using Serilog;
    using static LsideWPF.Models.Events;

    public class TakingOffState : State
    {
        public override void Initilize()
        {
            // set up for next landing
            this.stateMachine.LandingResponses.Clear();
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
                    this.stateMachine.TransitionTo(new TaxingState());
                    return;
                }
            }
            else if (planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // now flying (above 100 ft)
                FlightEventArgs e = new FlightEventArgs
                {
                    EventType = EventType.TakeOffEvent,
                    StateMachine = new StateMachine(this.stateMachine),
                };

                this.eventHandler?.Invoke(this, e);

                this.stateMachine.TransitionTo(new FlyingState());
            }
        }
    }
}
