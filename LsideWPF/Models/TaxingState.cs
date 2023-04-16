namespace LsideWPF.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class TaxingState : State
    {
        public override void Initilize()
        {
            // set up for next landing
            this.StateMachine.LandingResponses.Clear();
            Log.Debug("Taxing State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (!planeInfoResponse.OnGround)
            {
                this.StateMachine.TransitionTo(new TakingOffState());
            }
            else
            {
                if (planeInfoResponse.GroundSpeed > Properties.Settings.Default.MaxTaxiSpeedKts)
                {
                    // now taxing (below 30Kts)
                    this.StateMachine.TransitionTo(new TakingOffState());
                }
            }
        }
    }
}
