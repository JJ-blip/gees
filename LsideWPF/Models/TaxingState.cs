namespace LsideWPF.Models
{
    using LsideWPF.Common;
    using Serilog;

    public class TaxingState : State
    {
        override public void Initilize()
        {
            // set up for next landing
            this.stateMachine.LandingResponses.Clear();
            Log.Debug("Taxing State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (!planeInfoResponse.OnGround)
            {
                this.stateMachine.TransitionTo(new TakingOffState());
            }
            else
            {
                if (planeInfoResponse.GroundSpeed > Properties.Settings.Default.MaxTaxiSpeedKts)
                {
                    // write Slip data to file if enabled
                    this.slipLogger?.WriteLogToFile();

                    // now taxing (below 30Kts)
                    this.stateMachine.TransitionTo(new TakingOffState());
                }
            }
        }
    }
}
