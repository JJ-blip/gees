namespace GeesWPF.model
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
