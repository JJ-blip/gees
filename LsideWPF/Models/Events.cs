namespace LsideWPF.Models
{
    using System;
    using LsideWPF.Common;

    public class Events
    {
        public enum EventType
        {
            // requires: StateMachine populated
            TakeOffEvent,
            TouchAndGoEvent,
            LandingEvent,

            // requires: PlaneInfoResponse poplated
            SlipLoggingEvent,
        }

        public class FlightEventArgs : EventArgs
        {
            public EventType EventType;

            public StateMachine StateMachine;

            public PlaneInfoResponse PlaneInfoResponse;
        }
    }
}
