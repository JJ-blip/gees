namespace LsideWPF.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class FlyingState : State
    {
        private readonly ISlipLogger slipLogger = App.Current.Services.GetService<ISlipLogger>();

        // say 500 feet
        private readonly int slipLoggingThresholdFeet = Properties.Settings.Default.SlipLoggingThresholdFt;

        private bool startedSlipLogger = false;

        public override void Initilize()
        {
            this.StateMachine.Bounces = 0;
            Log.Debug("Flying State");
        }

        /// <param name="planeInfoResponse">simulation data.</param>
        public override void Handle(PlaneInfoResponse planeInfoResponse)
        {
            // if - in the air below 500ft with wheels down
            if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround < this.slipLoggingThresholdFeet && this.IsLanding(planeInfoResponse))
            {
                if (!this.startedSlipLogger)
                {
                    // just dropped below 500 feet
                    this.startedSlipLogger = true;
                    this.slipLogger.BeginLogging();
                }

                // do the slip logging between the keep logging between flying below slipLoggingThresholdFeet (500 ft), while isLanding
                this.slipLogger.Log(planeInfoResponse);
            }

            if (this.startedSlipLogger && planeInfoResponse.AltitudeAboveGround >= this.slipLoggingThresholdFeet)
            {
                // reset & cancel if we go back up above 500 feet
                this.startedSlipLogger = false;
                this.slipLogger.CancelLogging();
            }

            if (!planeInfoResponse.OnGround && planeInfoResponse.AltitudeAboveGround > Properties.Settings.Default.LandingThresholdFt)
            {
                // still flying (above 100 ft)
            }
            else
            {
                // now below 100ft
                this.StateMachine.TransitionTo(new LandingState());
            }
        }

        private bool IsLanding(PlaneInfoResponse response)
        {
            if (response.GearPosition == 2 || response.VerticalSpeed >= 0)
            {
                // unambigiously-up
                return false;
            }

            if (response.GearPosition == 1 || response.LightLandingOn)
            {
                // unambigiously-down or indicative intention to land
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
