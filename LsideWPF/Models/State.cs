namespace LsideWPF.Services
{
    using LsideWPF.Services;

    public abstract class State
    {
        protected StateMachine stateMachine;

        public void SetStateMachine(StateMachine context)
        {
            this.stateMachine = context;
        }

        /// <summary>
        /// Handle a new packet of simulation data
        /// </summary>
        /// <param name="planeInfoResponse"></param>
        public abstract void Handle(PlaneInfoResponse planeInfoResponse);

        public virtual void Initilize()
        {
        }
    }
}
