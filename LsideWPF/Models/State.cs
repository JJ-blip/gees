using LsideWPF.Common;
using System;
using static LsideWPF.Models.Events;

namespace LsideWPF.Models
{
    public abstract class State
    {
        protected StateMachine _context;
        protected EventHandler<FlightEventArgs> eventHandler;
        protected SlipLogger slipLogger;

        public void SetContext(StateMachine context, EventHandler<FlightEventArgs> eventHandler)
        {
            this._context = context;
            this.eventHandler = eventHandler;
        }

        public abstract void Handle(PlaneInfoResponse planeInfoResponse);

        public virtual void Initilize() { }
    }
}
