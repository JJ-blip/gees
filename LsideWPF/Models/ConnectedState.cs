namespace LsideWPF.Services
{
    using LsideWPF.Services;

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
