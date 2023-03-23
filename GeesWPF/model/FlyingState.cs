using System.Diagnostics;

namespace GeesWPF.model
{
    class FlyingState : State
    {
        override public void Initilize()
        {
            this._context.Bounces = 0;
            Debug.WriteLine("Flying State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (!planeInfoResponse.OnGround)
            {
                // do nothing
            }
            else if (planeInfoResponse.AltitudeAboveGround <= 100)
            {
                this._context.TransitionTo(new LandingState());
            }
        }
    }
}
