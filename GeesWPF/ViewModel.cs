using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace GeesWPF
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool updatable = false;

        public ViewModel()
        {
            Connected = false;
            updatable = false;
        }
        #region Main Form Data
        public string Version
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                string myversion = fvi.FileVersion;
                return myversion;
            }
        }

        bool connected;
        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
                OnPropertyChanged();
            }
        }

        public string ConnectedString
        {
            get
            {
                if (Connected)
                {
                    return "Connected";
                }
                else
                {
                    return "Disconnected";
                }
            }
        }

        public string ConnectedColor
        {
            get
            {
                if (!Connected)
                {
                    return "#FFE63946";
                }
                else
                {
                    return "#ff02c39a";
                }
            }
        }

        public bool Updatable
        {
            get
            {
                return updatable;
            }
            set
            {
                updatable = value;
                OnPropertyChanged();
            }
        }

        public List<int> Displays
        {
            get
            {
                List<int> displays = new List<int>();
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    displays.Add(i + 1);
                }
                return displays;
            }
        }
        #endregion



        #region Landing Rate Data
        public class Parameters
        {
            public string Name { get; set; }
            public int FPM { get; set; }
            public double Gforce { get; set; }
            public double Airspeed { get; set; }
            public double Groundspeed { get; set; }
            public double Headwind { get; set; }
            public double Slip { get; set; }
            public double Crosswind { get; set; }
            public int Bounces { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double LandingDistance { get; set; }

        }


        public Parameters _parameters = new Parameters
        {
            Name = null,
            FPM = -0,
            Gforce = 0,
            Airspeed = 0,
            Groundspeed = 0,
            Headwind = -0,
            Crosswind = 0,
            Slip = 0,
            Bounces = 0,
            LandingDistance = 0
        };

        public void SetParams(Parameters value)
        {
            _parameters = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void BounceParams()
        {
            _parameters.Bounces += 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public string LandingText
        {
            get { return _parameters.LandingDistance.ToString("0 m"); }
        }
        public string FPMText
        {
            get { return _parameters.FPM.ToString("0 fpm"); }
        }
        public string GforceText
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
        public string SpeedsText
        {
            get { return String.Format("{0} kt Air - {1} kt Ground", Convert.ToInt32(_parameters.Airspeed), Convert.ToInt32(_parameters.Groundspeed)); }
        }
        public string WindSpeedText
        {
            get
            {
                double Crosswind = _parameters.Crosswind;
                double Headwind = _parameters.Headwind;
                double windamp = Math.Sqrt(Crosswind * Crosswind + Headwind * Headwind);
                return Convert.ToInt32(windamp) + " kt";
            }
        }
        public int WindDirection
        {
            get
            {
                double Crosswind = _parameters.Crosswind;
                double Headwind = _parameters.Headwind;
                double windangle = Math.Atan2(Crosswind, Headwind) * 180 / Math.PI;
                return Convert.ToInt32(windangle);
            }
        }
        public string AlphaText
        {
            get { return _parameters.Slip.ToString("0.##º Left Sideslip; 0.##º Right Sideslip;"); }
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

        #endregion

        public void SetParms(model.StateMachine stateMachine)
        {
            if (stateMachine == null) return;

            bool isLandingParameters = false;

            try
            {
                LinkedList<PlaneInfoResponse> displayResponses;
                if (stateMachine.landingResponses.Count > 0)
                {
                    // normal case display landing data
                    displayResponses = stateMachine.landingResponses;
                    isLandingParameters = true;
                }
                else
                {
                    // special case just display the last sampled data 
                    displayResponses = stateMachine.responses;
                }

                // display the contents
                PlaneInfoResponse response = displayResponses.FirstOrDefault();

                double fpm = 60 * response.LandingRate;
                Int32 FPM = Convert.ToInt32(-fpm);

                // compute g force, taking largest value
                double gforce = 0;
                foreach (var resp in displayResponses)
                {
                    if (resp.Gforce > gforce)
                    {
                        gforce = resp.Gforce;
                    }
                }

                // compute when traveling
                double incAngle = 0;
                if (response.SpeedAlongHeading > 5)
                {
                    incAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;
                }

                Parameters parameters = new ViewModel.Parameters
                {
                    Name = response.Type,

                    Airspeed = Math.Round(response.AirspeedInd, 2),
                    Groundspeed = Math.Round(response.GroundSpeed, 2),
                    Crosswind = Math.Round(response.WindSpeedLat, 2),
                    Headwind = Math.Round(response.WindSpeedAlongHeading, 2),
                    Slip = Math.Round(incAngle, 2),
                    Bounces = stateMachine.Bounces,
                    Latitude = Math.Round(response.Latitude, 2),
                    Longitude = Math.Round(response.Longitude, 2)
                };

                if (isLandingParameters)
                {
                    parameters.FPM = FPM;
                    parameters.Gforce = Math.Round(gforce, 2);
                    parameters.LandingDistance = stateMachine.landingDistance;
                }
                this.SetParams(parameters);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //some params are missing. likely the user is in the main menu. ignore
            }
        }

        protected void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }
}
