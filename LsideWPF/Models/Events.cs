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
            private readonly EventType eventType;

            private readonly StateMachine stateMachine;

            private readonly PlaneInfoResponse planeInfoResponse;

            public FlightEventArgs(EventType eventType, StateMachine stateMachineClone)
            {
                this.eventType = eventType;
                this.stateMachine = stateMachineClone;
            }

            public FlightEventArgs(EventType eventType, PlaneInfoResponse planeInfoResponse)
            {
                this.eventType = eventType;
                this.planeInfoResponse = planeInfoResponse;
            }

            public EventType EventType()
            {
                return this.eventType;
            }

            public StateMachine StateMachine()
            {
                return this.stateMachine;
            }

            public PlaneInfoResponse PlaneInfoResponse()
            {
                return this.planeInfoResponse;
            }
        }
    }
}
