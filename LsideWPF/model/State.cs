using System;
using static LsideWPF.model.Events;

namespace LsideWPF.model
{
    public abstract class State
    {
        protected StateMachine _context;
        protected EventHandler<FlightEventArgs> eventHandler;

        public void SetContext(StateMachine context, EventHandler<FlightEventArgs> eventHandler)
        {
            this._context = context;
            this.eventHandler = eventHandler;
        }

        public abstract void Handle(PlaneInfoResponse planeInfoResponse);

        public virtual void Initilize() { }
    }
}
