namespace LsideWPF.Models
{
    using LsideWPF.Common;

    public abstract class State
    {
        protected StateMachine StateMachine { get; private set; }

        public void SetStateMachine(StateMachine context)
        {
            this.StateMachine = context;
        }

        /// <summary>
        /// Handle a new packet of simulation data.
        /// </summary>
        /// <param name="planeInfoResponse">simulation data.</param>
        public abstract void Handle(PlaneInfoResponse planeInfoResponse);

        public virtual void Initilize()
        {
        }
    }
}
