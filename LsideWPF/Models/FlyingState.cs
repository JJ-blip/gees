namespace LsideWPF.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using static LsideWPF.Services.Events;

    public class FlyingState : State
    {
        private readonly ISlipLogger slipLogger = App.Current.Services.GetService<ISlipLogger>();

        public override void Initilize()
        {
            this.StateMachine.Bounces = 0;
            Log.Debug("Flying State");
        }

        /// <param name="planeInfoResponse">simulation data.</param>
        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround < Properties.Settings.Default.SlipLoggingThresholdFt && planeInfoResponse.LandingGearDown && planeInfoResponse.RelativeWindVelocityBodyY < -100)
            {
                this.slipLogger.Reset();

                // no change of state, but if enabled, pass current data to event handler
                FlightEventArgs e = new FlightEventArgs(EventType.SlipLoggingEvent, planeInfoResponse);
                this.StateMachine.EventPublisherHandler?.Invoke(this, e);
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
                this.StateMachine.TransitionTo(new LandingState());
            }
        }
    }
}
