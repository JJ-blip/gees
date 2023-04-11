using LsideWPF.Common;
using System;

namespace LsideWPF.Models
{
    public class Events
    {
        public enum EventType
        {
            // requires: StateMachine populated
            TakeOffEvent,
            TouchAndGoEvent,
            LandingEvent,

            // requires: PlaneInfoResponse poplated
            SlipLoggingEvent
        }

        public class FlightEventArgs : EventArgs
        {
            public EventType eventType;
            public StateMachine stateMachine;

            public PlaneInfoResponse planeInfoResponse;
        }
    }
}
