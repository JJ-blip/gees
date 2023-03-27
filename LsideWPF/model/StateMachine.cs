using System;
using static LsideWPF.model.Events;

namespace LsideWPF.model
{
    public class StateMachine
    {
        private State _state = null;

        readonly EventHandler<FlightEventArgs> eventHandler = null;

        // only landing responses
        public FillOnceBuffer<PlaneInfoResponse> landingResponses = new FillOnceBuffer<PlaneInfoResponse>(6);

        // all, most recent response, any state
        public LifoBuffer<PlaneInfoResponse> responses = new LifoBuffer<PlaneInfoResponse>(2);

        // accumulated bounces
        public int Bounces = 0;

        public double SlowingDistance;
        public string arrivalAirport;
        public double DistanceFromAimingPoint;
        public double OffsetFromCenterLine;
        public double bankAngle;

        public double takeoffDistance;

        public StateMachine(StateMachine stateMachine)
        {
            this.eventHandler = stateMachine.eventHandler;
            this.landingResponses = new FillOnceBuffer<PlaneInfoResponse>(stateMachine.landingResponses);
            this.responses = new LifoBuffer<PlaneInfoResponse>(stateMachine.responses);
            this.Bounces = stateMachine.Bounces;
            this.SlowingDistance = stateMachine.SlowingDistance;
            this.takeoffDistance = stateMachine.takeoffDistance;
        }

        public StateMachine(State state, EventHandler<FlightEventArgs> eventHandler)
        {
            this.TransitionTo(state);
            this.eventHandler = eventHandler;
        }

        public void TransitionTo(State state)
        {
            this._state = state;
            this._state.SetContext(this, eventHandler);
            this._state.Initilize();
        }

        public void Handle(PlaneInfoResponse planeInfoResponse)
        {
            responses.Add(planeInfoResponse);

            if (_state is LandingState && planeInfoResponse.OnGround)
            {
                // will capture one set of responses for this landing state
                landingResponses.Add(planeInfoResponse);
            }

            this._state.Handle(planeInfoResponse);
        }
    }
}

