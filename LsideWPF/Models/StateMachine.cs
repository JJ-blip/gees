namespace LsideWPF.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LsideWPF.Common;
    using LsideWPF.Utils;
    using static LsideWPF.Models.Events;

    public class StateMachine
    {
        // The current state
        private State state = null;

        // This is just a snapshot clone of the main statemachine,
        // for handing off to the publishing handler
        public StateMachine(StateMachine stateMachine)
        {
            // stick with same publishing handler
            this.EventPublisherHandler = stateMachine.EventPublisherHandler;

            // new buffers caring the original response data
            this.LandingResponses = new FillOnceBuffer<PlaneInfoResponse>(stateMachine.LandingResponses);
            this.Responses = new BoundedQueue<PlaneInfoResponse>(stateMachine.Responses);

            // but carry over the computed properties
            this.Bounces = stateMachine.Bounces;
            this.SlowingDistance = stateMachine.SlowingDistance;
            this.TakeoffDistance = stateMachine.TakeoffDistance;
        }

        // The main constructor, takes an initial state and the states publishing handler
        public StateMachine(State initialState, EventHandler<FlightEventArgs> eventHandler)
        {
            this.TransitionTo(initialState);
            this.EventPublisherHandler = eventHandler;

            this.LandingResponses = new FillOnceBuffer<PlaneInfoResponse>(6);
            this.Responses = new BoundedQueue<PlaneInfoResponse>(2);
        }

        // Interface to the external enviroment through which Messages are published
        // given an EventType and some flightParameters.
        public EventHandler<FlightEventArgs> EventPublisherHandler { get; protected set; }

        // state memory - only landing responses
        public FillOnceBuffer<PlaneInfoResponse> LandingResponses { get; protected set; }

        // state memory - most recent response, any state
        public BoundedQueue<PlaneInfoResponse> Responses { get; protected set; }

        // - state memory - snapshot of key attributes
        public string ArrivalAirport { get; protected set; }

        public double DistanceFromAimingPoint { get; protected set; }

        public double OffsetFromCenterLine { get; protected set; }

        public double BankAngle { get; protected set; }

        public double TakeoffDistance { get; protected set; }

        // accumulated bounces - computed with a State
        public int Bounces { get; set; }

        // computed SlowingDistance - computed with a State
        public double SlowingDistance { get; set; }

        public double AverageHeadwind { get; private set; }

        public double AverageCrosswind { get; private set; }

        public void TransitionTo(State newState)
        {
            // carry over the machine into new state
            newState.SetStateMachine(this);

            // tell the machine the new state
            this.state = newState;

            // let the state do its own initilisation
            this.state.Initilize();
        }

        /// <summary>
        /// Takes a new data packet from the simulator and asks the current state to process it.
        ///
        ///   Adds the planeInfoResponse to the state memory if appropriate
        ///   Hands off the handling to the current states own handle method.
        ///
        /// <param name="planeInfoResponse">Data from the simulator to be handled</param>
        /// </summary>
        public void Handle(PlaneInfoResponse planeInfoResponse)
        {
            this.Responses.Enqueue(planeInfoResponse);

            if (this.state is LandingState && planeInfoResponse.OnGround)
            {
                // will capture one set of responses for this landing state, first added (last position) will be most significant.
                this.LandingResponses.Add(planeInfoResponse);
            }

            this.state.Handle(planeInfoResponse);
        }

        public void ComputeHeadAndTailWinds()
        {
            this.AverageHeadwind = GetAverageHeadwind(this);
            this.AverageCrosswind = GetAverageCrosswind(this);
        }

        private static double GetAverageHeadwind(StateMachine stateMachine)
        {
            // average from nominally 6 samples near ground
            LinkedList<PlaneInfoResponse> responses = stateMachine.LandingResponses;
            double result = -responses.Average(r => r.AircraftWindZ);
            return result;
        }

        private static double GetAverageCrosswind(StateMachine stateMachine)
        {
            // average from nominally 6 samples near ground
            LinkedList<PlaneInfoResponse> responses = stateMachine.LandingResponses;
            double result = responses.Average(r => r.AircraftWindX);
            return result;
        }
    }
}
