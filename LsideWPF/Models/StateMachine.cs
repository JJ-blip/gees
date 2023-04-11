﻿using LsideWPF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static LsideWPF.Models.Events;

namespace LsideWPF.Models
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

        // returns FlightParameters or null
        public static FlightParameters ToFlightParameters(StateMachine stateMachine)
        {
            FlightParameters parameters = null;

            if (stateMachine == null) return parameters;

            try
            {
                LinkedList<PlaneInfoResponse> responses = stateMachine.landingResponses;
                if (stateMachine.landingResponses.Count > 0)
                {
                    var response = responses.FirstOrDefault();
                    double fpm = 60 * response.LandingRate;
                    Int32 FPM = Convert.ToInt32(-fpm);

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
                //some params are missing. likely the user is in the main menu. ignore
                return null;
            }
        }

    }
}

