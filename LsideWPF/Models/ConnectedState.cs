namespace LsideWPF.Models
{
    using LsideWPF.Common;

    public class ConnectedState : State
    {
        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (planeInfoResponse.OnGround)
            {
                this.stateMachine.TransitionTo(new TaxingState());
            }
            else
            {
                this.stateMachine.TransitionTo(new FlyingState());
            }
        }
    }
}
