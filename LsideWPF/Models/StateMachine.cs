namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LsideWPF.Services;
    using static LsideWPF.Services.Events;

    public class StateMachine
    {
        // The current state
        private State state = null;

        // Interface to the external enviroment through which Messages are published
        // given an EventType and some flightParameters.
        public readonly EventHandler<FlightEventArgs> eventPublisherHandler = null;

        // state memory - only landing responses
        public FillOnceBuffer<PlaneInfoResponse> LandingResponses = new FillOnceBuffer<PlaneInfoResponse>(6);

        // state memory - most recent response, any state
        public LifoBuffer<PlaneInfoResponse> responses = new LifoBuffer<PlaneInfoResponse>(2);

        // - state memory - snapshot of key attributes
        // accumulated bounces
        public int Bounces = 0;
        public double SlowingDistance;
        public string ArrivalAirport;
        public double DistanceFromAimingPoint;
        public double OffsetFromCenterLine;
        public double BankAngle;
        public double TakeoffDistance;

        // This is just a lightweight clone of the main statemachine for handing off to the publishing handler
        public StateMachine(StateMachine stateMachine)
        {
            // stick with same publishing handler
            this.eventPublisherHandler = stateMachine.eventPublisherHandler;

            // clear history
            this.LandingResponses = new FillOnceBuffer<PlaneInfoResponse>(stateMachine.LandingResponses);
            this.responses = new LifoBuffer<PlaneInfoResponse>(stateMachine.responses);

            // but carry over the computed properties
            this.Bounces = stateMachine.Bounces;
            this.SlowingDistance = stateMachine.SlowingDistance;
            this.TakeoffDistance = stateMachine.TakeoffDistance;
        }

        // The main constructor, takes an initial state and the states publishing handler
        public StateMachine(State initialState, EventHandler<FlightEventArgs> eventHandler)
        {
            this.TransitionTo(initialState);
            this.eventPublisherHandler = eventHandler;
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
        /// Takes a new data packet from the simulator a & asks the current state to process it.
        ///
        ///   Adds the planeInfoResponse to the state memory if appropriate
        ///   Hands off the handling to the current states own handle method.
        ///
        /// <param name="planeInfoResponse">Data from the simulator to be handled</param>
        /// </summary>
        public void Handle(PlaneInfoResponse planeInfoResponse)
        {
            this.responses.Add(planeInfoResponse);

            if (this.state is LandingState && planeInfoResponse.OnGround)
            {
                // will capture one set of responses for this landing state
                this.LandingResponses.Add(planeInfoResponse);
            }

            this.state.Handle(planeInfoResponse);
        }

        /// <summary>
        /// Returns the most recent flightParameters read from the state memory.
        ///
        /// <param name="stateMachine"></param>
        /// <returns>flightParameters or null</returns>
        /// </summary>
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
                    var response = responses.FirstOrDefault();
                    double fpm = 60 * response.LandingRate;
                    int FPM = Convert.ToInt32(-fpm);

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

                    double slipAngle = Math.Atan(response.CrossWind / response.HeadWind) * 180 / Math.PI;

                    parameters = new FlightParameters
                    {
                        Name = response.Type,

                        AirSpeedInd = Math.Round(response.AirspeedInd, 1),
                        GroundSpeed = Math.Round(response.GroundSpeed, 1),
                        CrossWind = Math.Round(response.CrossWind, 1),

                        // A positive velocity is defined to be toward the tail
                        HeadWind = -Math.Round(response.HeadWind, 1),
                        SlipAngle = Math.Round(driftAngle, 1),

                        // read the accumulated bouces
                        Bounces = stateMachine.Bounces,
                        Latitude = Math.Round(response.Latitude, 1),
                        Longitude = Math.Round(response.Longitude, 1),
                        FPM = FPM,
                        Gforce = Math.Round(gforce, 1),
                        SlowingDistance = Convert.ToInt32(Math.Truncate(stateMachine.SlowingDistance)),
                        BankAngle = Math.Round(response.PlaneBankDegrees, 1),
                        AimPointOffset = Convert.ToInt32(Math.Truncate(response.AtcRunwayTdpointRelativePositionZ)),
                        CntLineOffser = Convert.ToInt32(Math.Truncate(response.AtcRunwayTdpointRelativePositionX)),
                        Airport = response.AtcRunwayAirportName,
                        RelativeWindVelocityBodyX = Math.Round(response.RelativeWindVelocityBodyX, 2),
                        RelativeWindVelocityBodyZ = Math.Round(response.RelativeWindVelocityBodyZ, 2),
                        DriftAngle = Math.Round(driftAngle, 1),
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
    }
}
