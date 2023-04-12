namespace LsideWPF.Services
{
    using LsideWPF.Services;
    using Serilog;
    using static LsideWPF.Services.Events;

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
                FlightEventArgs e = new FlightEventArgs(EventType.TakeOffEvent, new StateMachine(this.stateMachine));

                this.stateMachine.eventPublisherHandler?.Invoke(this, e);

                this.stateMachine.TransitionTo(new FlyingState());
            }
        }
    }
}
