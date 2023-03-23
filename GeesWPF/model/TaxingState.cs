using System.Diagnostics;

namespace GeesWPF.model
{
    class TaxingState : State
    {
        override public void Initilize()
        {
            // set up for next landing
            this._context.landingResponses.Clear();
            Debug.WriteLine("Taxing State");
        }

        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (!planeInfoResponse.OnGround)
            {
                this._context.TransitionTo(new TakingOffState());
            }
            else
            {
                if (planeInfoResponse.GroundSpeed > 30)
                {
                    this._context.TransitionTo(new TakingOffState());
                }
            }
        }
    }
}
