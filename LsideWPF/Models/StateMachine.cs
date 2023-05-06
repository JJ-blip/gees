namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LsideWPF.Utils;
    using static LsideWPF.Services.Events;

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

        /// <summary>
        /// Returns the most recent flightParameters read from the state memory.
        ///
        /// <param name="stateMachine"></param>
        /// </summary>
        /// <returns>FlightParameters or null.</returns>
        public static FlightParameters GetMostRecentLandingFlightParameters(StateMachine stateMachine)
        {
            FlightParameters parameters = null;

            if (stateMachine == null)
            {
                return parameters;
            }

            try
            {
                LinkedList<PlaneInfoResponse> responses = stateMachine.LandingResponses;
                if (stateMachine.LandingResponses.Count > 0)
                {
                    // user wants average over approach not value at touchdown.
                    double averageHeadwind = responses.Average(r => r.AircraftWindZ);
                    double averageCrosswind = responses.Average(r => r.AircraftWindX);

                    var response = responses.FirstOrDefault();
                    double fpm = 60 * response.LandingRate;
                    int ifpm = Convert.ToInt32(-fpm);

                    // compute g force, taking largest value
                    double gforce = 0;
                    foreach (var resp in responses)
                    {
                        if (resp.Gforce > gforce)
                        {
                            gforce = resp.Gforce;
                        }
                    }

                    // compute when traveling
                    double driftAngle = 0;
                    if (response.SpeedAlongHeading > 5)
                    {
                        driftAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;
                    }

                    double slipAngle = Math.Atan(response.RelativeWindX / response.RelativeWindZ) * 180 / Math.PI;

                    parameters = new FlightParameters
                    {
                        Name = response.Type,

                        AirSpeedInd = Math.Round(response.AirspeedInd, 1),
                        GroundSpeed = Math.Round(response.GroundSpeed, 1),
                        RelativeWindX = Math.Round(response.RelativeWindX, 1),
                        RelativeWindZ = Math.Round(response.RelativeWindZ, 1),
                        SlipAngle = Math.Round(slipAngle, 1),

                        // read the accumulated bouces
                        Bounces = stateMachine.Bounces,

                        Latitude = Math.Round(response.Latitude, 1),
                        Longitude = Math.Round(response.Longitude, 1),
                        FPM = ifpm,
                        Gforce = Math.Round(gforce, 1),
                        SlowingDistance = Convert.ToInt32(Math.Truncate(stateMachine.SlowingDistance)),
                        BankAngle = Math.Round(response.PlaneBankDegrees, 1),
                        AimPointOffset = Convert.ToInt32(Math.Truncate(response.AtcRunwayTdpointRelativePositionZ)),
                        CntLineOffser = Convert.ToInt32(Math.Truncate(response.AtcRunwayTdpointRelativePositionX)),
                        Airport = response.AtcRunwayAirportName,
                        DriftAngle = Math.Round(driftAngle, 1),
                        AircraftWindX = averageCrosswind,
                        AircraftWindZ = averageHeadwind,
                    };
                }

                return parameters;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                // some params are missing. likely the user is in the main menu. ignore
                return null;
            }
        }

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
                // will capture one set of responses for this landing state
                this.LandingResponses.Add(planeInfoResponse);
            }

            this.state.Handle(planeInfoResponse);
        }
    }
}
