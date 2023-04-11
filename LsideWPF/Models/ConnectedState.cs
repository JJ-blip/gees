

using LsideWPF.Common;

namespace LsideWPF.Models
{
    class ConnectedState : State
    {
        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (planeInfoResponse.OnGround)
            {
                this._context.TransitionTo(new TaxingState());
            }
            else
            {
                this._context.TransitionTo(new FlyingState());
            }

        }
    }
}
