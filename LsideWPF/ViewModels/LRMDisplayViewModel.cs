using CommunityToolkit.Mvvm.Messaging;
using LsideWPF.Common;
using LsideWPF.Services;
using LsideWPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LsideWPF.ViewModels
{
    public class LRMDisplayViewModel : BindableBase
    {
        private ILandingLoggerService landingLogger = App.Current.Services.GetService<ILandingLoggerService>();

        const string AllHaveChanged = "";

        public LRMDisplayViewModel()
        {
            _parameters = new FlightParameters
            {
                Name = "",
                Airport = ""
            };

            // listen on ..
            WeakReferenceMessenger.Default.Register<LRMDisplayViewModel, ShowLastLandingMessage>(this, (sender, args) =>
            {
                ShowLastLandingMessageMessage();
            });

            // Listen on ..
            WeakReferenceMessenger.Default.Register<LRMDisplayViewModel, LandingEventMessage>(this, (r, m) =>
            {    
                // we could take fp from csv, but take from message - just because we can!
                ShowLandingMessageMessage(m);                
            });
        }

        private void ShowLandingMessageMessage(LandingEventMessage m)
        {
            var flightParameters = m.Value;
            SetParameters(flightParameters);
            WeakReferenceMessenger.Default.Send<SlideLeftMessage>();
        }

        private void ShowLastLandingMessageMessage()
        {
            var flightParameters = landingLogger.GetLastLandingFromCVS();
            SetParameters(flightParameters);
            WeakReferenceMessenger.Default.Send<SlideLeftMessage>();
        }

        public FlightParameters _parameters;

        public void SetParameters(FlightParameters value)
        {
            _parameters = value;
            // all have changed
            OnPropertyChanged(AllHaveChanged);
        }

        public string StoppingDistText
        {
            get { return _parameters.SlowingDistance.ToString("0 ft"); }
        }
        public string FPMText
        {
            get { return _parameters.FPM.ToString("0 fpm"); }
        }
        public string GForceText
        {
            get { return _parameters.Gforce.ToString("0.## G"); }
        }
        public string GforceImage
        {
            get
            {
                if (_parameters.Gforce < 1.2)
                {
                    return "/Images/smile.png";
                }
                else if (_parameters.Gforce < 1.4)
                {
                    return "/Images/meh.png";
                }
                else if (_parameters.Gforce < 1.8)
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
            get { return String.Format("{0} kt Air - {1} kt Ground", Convert.ToInt32(_parameters.AirSpeedInd), Convert.ToInt32(_parameters.GroundSpeed)); }
        }
        public string WindSpeedText
        {
            get
            {
                double Crosswind = _parameters.CrossWind;
                double Headwind = _parameters.HeadWind;
                double windamp = Math.Sqrt(Crosswind * Crosswind + Headwind * Headwind);
                return Convert.ToInt32(windamp) + " kt";
            }
        }
        public int WindDirection
        {
            get
            {
                double Crosswind = _parameters.CrossWind;
                double Headwind = _parameters.HeadWind;
                double windangle = Math.Atan2(Crosswind, Headwind) * 180 / Math.PI;
                return Convert.ToInt32(windangle);
            }
        }

        public string DriftText
        {
            get { return _parameters.DriftAngle.ToString("0.##º Left Drift; 0.##º Right Drift; 0º Drift"); }
        }

        public string SlipText
        {
            get { return _parameters.SlipAngle.ToString("0.##º Left Slip; 0.##º Right Slip; 0º Slip"); }
        }

        public string BouncesText
        {
            get
            {
                string unit = " bounces";
                if (_parameters.Bounces == 1)
                {
                    unit = " bounce";
                }
                return _parameters.Bounces.ToString() + unit;
            }
        }


        public string DistFromTarget
        {
            get
            {
                if (_parameters.Airport == "")
                    return "";
                else
                    return _parameters.AimPointOffset.ToString("0 ft beyond; 0 ft short; 0 ft bang on!");
            }
        }

        public string DistFromCntLine
        {
            get
            {
                if (_parameters.Airport == "")
                    return "";
                else
                    return _parameters.CntLineOffser.ToString("0 m right; 0 ft left ; 0 ft bang on!");
            }
        }

        public string BankAngleText
        {
            get
            {
                double bankAngle = _parameters.BankAngle;
                return bankAngle.ToString(" 0.#º left; 0.#º right; 0º ");
            }
        }

        public string HeadWindText
        {
            get
            {
                double HeadWind = _parameters.HeadWind;
                return Convert.ToInt32(HeadWind) + " Kts";
            }
        }

        public string CrossWindText
        {
            get
            {
                double Crosswind = _parameters.CrossWind;
                return Convert.ToInt32(Crosswind).ToString(" # kts (from left); # kts (from right); 0 Kts ");
            }
        }

        public string PlaneText
        {
            get { return _parameters.Name; }
        }

        public string AirportText
        {
            get
            {
                if (_parameters.Airport == "")
                    return "";
                else
                    return _parameters.Airport;
            }
        }

    }
}
