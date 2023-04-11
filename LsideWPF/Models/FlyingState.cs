using LsideWPF.Common;
using Serilog;
using static LsideWPF.Models.Events;

namespace LsideWPF.Models
{
    class FlyingState : State
    {
        override public void Initilize()
        {
            this._context.Bounces = 0;
            slipLogger = null;

            Log.Debug("Flying State");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planeInfoResponse"></param>
        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {

            if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround < Properties.Settings.Default.SlipLoggingThresholdFt && planeInfoResponse.LandingGearDown && planeInfoResponse.RelativeWindVelocityBodyY < -100)
            {
                if (this.slipLogger == null)
                    slipLogger = new SlipLogger();

                // no change of state, but if enabled, pass current data to event handler
                FlightEventArgs e = new FlightEventArgs
                {
                    eventType = EventType.SlipLoggingEvent,
                    planeInfoResponse = planeInfoResponse
                };
                this.eventHandler?.Invoke(this, e);
            }

            if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // still flying (above 100 ft)

                /*
                {
                    double slipAngle = Math.Atan(planeInfoResponse.CrossWind / planeInfoResponse.HeadWind) * 180 / Math.PI;
                
                    var msg =
                          $"AltitudeAboveGround: {planeInfoResponse.AltitudeAboveGround} " 
                        + $"GroundVelocity: {planeInfoResponse.GroundSpeed} "
                        + $", LateralSpeed: {planeInfoResponse.LateralSpeed} "
                        + $", SpeedAlongHeading: {planeInfoResponse.SpeedAlongHeading} "
                        + $", HeadWind: {planeInfoResponse.HeadWind} "
                        + $", CrossWind: {planeInfoResponse.CrossWind} "
                        + $", RelativeWindVelocityBodyX: {planeInfoResponse.RelativeWindVelocityBodyX} "
                        + $", RelativeWindVelocityBodyZ: {planeInfoResponse.RelativeWindVelocityBodyZ} "
                        + $", AmbientWindX: {planeInfoResponse.AmbientWindX} "
                        + $", AmbientWindZ: {planeInfoResponse.AmbientWindZ} "                  
                        + $", slipAngle: {slipAngle}";

                    Log.Debug(msg);
                }                
                */
            }
            else
            {
                // now below 100ft 
                this._context.TransitionTo(new LandingState());
            }
        }
    }
}
