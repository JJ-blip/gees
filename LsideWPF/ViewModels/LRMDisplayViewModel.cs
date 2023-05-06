namespace LsideWPF.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.Messaging;
    using LsideWPF.Services;
    using Microsoft.Extensions.DependencyInjection;

    public class LRMDisplayViewModel : BindableBase
    {
        private const string AllHaveChanged = "";

        private readonly ILandingLoggerService landingLogger = App.Current.Services.GetService<ILandingLoggerService>();

        private FlightParameters flightParameters;

        public LRMDisplayViewModel()
        {
            this.flightParameters = new FlightParameters
            {
                Name = string.Empty,
                Airport = string.Empty,
            };

            // listen on ..
            WeakReferenceMessenger.Default.Register<LRMDisplayViewModel, ShowLastLandingMessage>(this, (sender, args) =>
            {
                this.ShowLastLandingMessageMessage();
            });

            // Listen on ..
            WeakReferenceMessenger.Default.Register<LRMDisplayViewModel, LandingEventMessage>(this, (r, m) =>
            {
                // we could take fp from csv, but take from message - just because we can!
                this.ShowLandingMessageMessage(m);
            });
        }

        public string StoppingDistText
        {
            get { return this.flightParameters.SlowingDistance.ToString("0 ft (to slow)"); }
        }

        public string FPMText
        {
            get { return this.flightParameters.FPM.ToString("0 fpm"); }
        }

        public string GForceText
        {
            get { return this.flightParameters.Gforce.ToString("0.## G"); }
        }

        public string GforceImage
        {
            get
            {
                if (this.flightParameters.Gforce < 1.2)
                {
                    return "/Images/smile.png";
                }
                else if (this.flightParameters.Gforce < 1.4)
                {
                    return "/Images/meh.png";
                }
                else if (this.flightParameters.Gforce < 1.8)
                {
                    return "/Images/frown.png";
                }
                else
                {
                    return "/Images/tired.png";
                }
            }
        }

        public string AirSpeedText
        {
            get { return string.Format("{0} kt Air - {1} kt Ground", Convert.ToInt32(this.flightParameters.AirSpeedInd), Convert.ToInt32(this.flightParameters.GroundSpeed)); }
        }

        public string DriftText
        {
            get { return this.flightParameters.DriftAngle.ToString("0.##º Left Drift; 0.##º Right Drift; 0º Drift"); }
        }

        public string SlipText
        {
            get { return this.flightParameters.SlipAngle.ToString("0.##º Left Slip; 0.##º Right Slip; 0º Slip"); }
        }

        public string BouncesText
        {
            get
            {
                string unit = " bounces";
                if (this.flightParameters.Bounces == 1)
                {
                    unit = " bounce";
                }

                return this.flightParameters.Bounces.ToString() + unit;
            }
        }

        public string DistFromTarget
        {
            get
            {
                if (this.flightParameters.Airport == string.Empty)
                {
                    return string.Empty;
                }
                else
                {
                    return this.flightParameters.AimPointOffset.ToString("0 ft beyond; 0 ft short; 0 ft bang on!");
                }
            }
        }

        public string DistFromCntLine
        {
            get
            {
                if (this.flightParameters.Airport == string.Empty)
                {
                    return string.Empty;
                }
                else
                {
                    return this.flightParameters.CntLineOffser.ToString("0 m right; 0 ft left ; 0 ft bang on!");
                }
            }
        }

        public string BankAngleText
        {
            get
            {
                double bankAngle = this.flightParameters.BankAngle;
                return bankAngle.ToString(" 0.#º left; 0.#º right; 0º ");
            }
        }

        public string HeadwindText
        {
            get
            {
                double aircraftWindZ = this.flightParameters.AircraftWindZ;
                return Convert.ToInt32(-aircraftWindZ).ToString(" # Kts (Headwind); # ktd (Tailwind); 0 Kts");
            }
        }

        public string AircraftWindXText
        {
            get
            {
                double aircraftWindX = this.flightParameters.AircraftWindX;
                return Convert.ToInt32(aircraftWindX).ToString(" # kts (from left); # kts (from right); 0 Kts ");
            }
        }

        public string PlaneText
        {
            get { return this.flightParameters.Name; }
        }

        public string AirportText
        {
            get
            {
                if (this.flightParameters.Airport == string.Empty)
                {
                    return string.Empty;
                }
                else
                {
                    return this.flightParameters.Airport;
                }
            }
        }

        public void SetParameters(FlightParameters value)
        {
            this.flightParameters = value;
            this.OnPropertyChanged(AllHaveChanged);
        }

        private void ShowLandingMessageMessage(LandingEventMessage m)
        {
            var flightParameters = m.Value;
            this.SetParameters(flightParameters);
            WeakReferenceMessenger.Default.Send<SlideLeftMessage>();
        }

        private void ShowLastLandingMessageMessage()
        {
            var flightParameters = this.landingLogger.GetLastLanding();
            this.SetParameters(flightParameters);
            WeakReferenceMessenger.Default.Send<SlideLeftMessage>();
        }
    }
}
