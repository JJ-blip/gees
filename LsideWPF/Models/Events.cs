namespace LsideWPF.Services
{
    using System;

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
            private EventType eventType;

            private StateMachine stateMachine;

            private PlaneInfoResponse planeInfoResponse;

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
