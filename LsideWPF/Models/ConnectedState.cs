namespace LsideWPF.Models
{
    using LsideWPF.Common;

    public class ConnectedState : State
    {
        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (planeInfoResponse.OnGround)
            {
                this.StateMachine.TransitionTo(new TaxingState());
            }
            else
            {
                this.StateMachine.TransitionTo(new FlyingState());
            }
        }
    }
}
