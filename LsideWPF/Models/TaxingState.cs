namespace LsideWPF.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class TaxingState : State
    {
        private ISlipLogger slipLogger = App.Current.Services.GetService<ISlipLogger>();

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
                    // write Slip data to file if enabled
                    this.slipLogger.WriteLogToFile();

                    // now taxing (below 30Kts)
                    this.StateMachine.TransitionTo(new TakingOffState());
                }
            }
        }
    }
}
