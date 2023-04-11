using LsideWPF.Common;
using Serilog;

namespace LsideWPF.Models
{
    class TaxingState : State
    {
        override public void Initilize()
        {
            // set up for next landing
            this._context.landingResponses.Clear();
            Log.Debug("Taxing State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (!planeInfoResponse.OnGround)
            {
                this._context.TransitionTo(new TakingOffState());
            }
            else
            {
                if (planeInfoResponse.GroundSpeed > Properties.Settings.Default.MaxTaxiSpeedKts)
                {
                    if (slipLogger != null)
                        // write Slip data to file if enabled
                        slipLogger.WriteLogToFile();

                    // now taxing (below 30Kts)
                    this._context.TransitionTo(new TakingOffState());
                }
            }
        }
    }
}
