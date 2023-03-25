using System;

namespace LsideWPF.model
{
    public class Events
    {
        public enum EventType
        {
            TakeOffEvent,
            TouchAndGoEvent,
            LandingEvent
        }

        public class FlightEventArgs : EventArgs
        {
            public EventType eventType;
            public StateMachine stateMachine;
        }
    }
}
