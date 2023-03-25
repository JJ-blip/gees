using Serilog;

namespace GeesWPF.model
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
                    // now taxing (below 30Kts)
                    this._context.TransitionTo(new TakingOffState());
                }
            }
        }
    }
}
