namespace LsideWPF.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.Messaging;
    using LsideWPF.Common;
    using LsideWPF.Models;
    using LsideWPF.Services;
    using Microsoft.Extensions.DependencyInjection;

    public class LRMDisplayViewModel : BindableBase
    {
        private const string AllHaveChanged = "";

        private readonly ILandingLoggerService landingLogger = App.Current.Services.GetService<ILandingLoggerService>();

        private LogEntry logEntry;

        public LRMDisplayViewModel()
        {
            this.logEntry = new LogEntry
            {
                Plane = string.Empty,
                Airport = string.Empty,
            };

            // listen on ..
            WeakReferenceMessenger.Default.Register<LRMDisplayViewModel, ShowLastLandingMessage>(this, (sender, args) =>
            {
                this.SendShowLastLandingMessage();
            });

            // Listen on ..
            WeakReferenceMessenger.Default.Register<LRMDisplayViewModel, LandingEventMessage>(this, (r, m) =>
            {
                // fp will have been saved - use it
                this.SendShowLastLandingMessage();
            });
        }

        public string StoppingDistText
        {
            get { return this.logEntry.SlowingDistance.ToString("0 ft (to slow)"); }
        }

        public string FPMText
        {
            get { return this.logEntry.Fpm.ToString("0 fpm"); }
        }

        public string GForceText
        {
            get { return this.logEntry.Gforce.ToString("0.## G"); }
        }

        public string GforceImage
        {
            get
            {
                if (this.logEntry.Gforce < 1.2)
                {
                    return "/Images/smile.png";
                }
                else if (this.logEntry.Gforce < 1.4)
                {
                    return "/Images/meh.png";
                }
                else if (this.logEntry.Gforce < 1.8)
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
            get { return string.Format("{0} kt Air - {1} kt Ground", Convert.ToInt32(this.logEntry.AirSpeedInd), Convert.ToInt32(this.logEntry.GroundSpeed)); }
        }

        public string DriftText
        {
            get { return this.logEntry.DriftAngle.ToString("0.##º Left Drift; 0.##º Right Drift; 0º Drift"); }
        }

        public string SlipText
        {
            get { return this.logEntry.SlipAngle.ToString("0.##º Left Slip; 0.##º Right Slip; 0º Slip"); }
        }

        public string BouncesText
        {
            get
            {
                string unit = " bounces";
                if (this.logEntry.Bounces == 1)
                {
                    unit = " bounce";
                }

                return this.logEntry.Bounces.ToString() + unit;
            }
        }

        public string DistFromTarget
        {
            get
            {
                if (!this.logEntry.LandedOnRunway)
                {
                    return string.Empty;
                }
                else
                {
                    return this.logEntry.AimPointOffset.ToString("0 ft beyond; 0 ft short; 0 ft bang on!");
                }
            }
        }

        public string DistFromCntLine
        {
            get
            {
                if (!this.logEntry.LandedOnRunway)
                {
                    return string.Empty;
                }
                else
                {
                    return this.logEntry.CntLineOffser.ToString("0 m right; 0 ft left ; 0 ft bang on!");
                }
            }
        }

        public string BankAngleText
        {
            get
            {
                double bankAngle = this.logEntry.BankAngle;
                return bankAngle.ToString(" 0.#º left; 0.#º right; 0º ");
            }
        }

        public string HeadwindText
        {
            get
            {
                double averageHeadwind = this.logEntry.AverageHeadwind;
                return Convert.ToInt32(averageHeadwind).ToString(" # Kts (Headwind); # kts (Tailwind); 0 Kts");
            }
        }

        public string CrosswindText
        {
            get
            {
                double averageCrosswind = this.logEntry.AverageCrosswind;
                return Convert.ToInt32(averageCrosswind).ToString(" # kts (from left); # kts (from right); 0 Kts ");
            }
        }

        public string PlaneText
        {
            get { return this.logEntry.Plane; }
        }

        public string AirportText
        {
            get
            {
                if (this.logEntry.Airport == string.Empty)
                {
                    return string.Empty;
                }
                else
                {
                    return this.logEntry.Airport;
                }
            }
        }

        public void SetParameters(LogEntry value)
        {
            this.logEntry = value;
            this.OnPropertyChanged(AllHaveChanged);
        }

        private void SendShowLastLandingMessage()
        {
            var logEntry = this.landingLogger.GetLastLanding();
            this.SetParameters(logEntry);
            WeakReferenceMessenger.Default.Send<SlideLeftMessage>();
        }
    }
}
