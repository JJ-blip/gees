using Serilog;

namespace LsideWPF.model
{
    class FlyingState : State
    {
        private int idx = 0;

        override public void Initilize()
        {
            this._context.Bounces = 0;
            Log.Debug("Flying State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        { 
            Log.Debug($"Flying: {idx++}, {planeInfoResponse}");

            if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // do nothing - still flying (above 100 ft)
            }
            else 
            {
                this._context.TransitionTo(new LandingState());
            }
        }
    }
}
