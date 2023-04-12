namespace LsideWPF.Models
{
    using System;
    using LsideWPF.Common;
    using static LsideWPF.Models.Events;

    public abstract class State
    {
        protected StateMachine stateMachine;
        protected EventHandler<FlightEventArgs> eventHandler;
        protected SlipLogger slipLogger;

        public void SetContext(StateMachine context, EventHandler<FlightEventArgs> eventHandler)
        {
            this.stateMachine = context;
            this.eventHandler = eventHandler;
        }

        public abstract void Handle(PlaneInfoResponse planeInfoResponse);

        public virtual void Initilize()
        {
        }
    }
}
