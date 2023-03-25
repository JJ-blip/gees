using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace LsideWPF
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
            public double AirSpeedInd { get; set; }
            public double GroundSpeed { get; set; }
            public double HeadWind { get; set; }
            public double Slip { get; set; }
            public double CrossWind { get; set; }
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
            AirSpeedInd = 0,
            GroundSpeed = 0,
            HeadWind = -0,
            CrossWind = 0,
            Slip = 0,
            Bounces = 0,
            LandingDistance = 0
        };

        public void SetParameters(Parameters value)
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

        public void SetParameters(model.StateMachine stateMachine)
        {
            if (stateMachine == null) return;

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
                    double incAngle = 0;
                    if (response.SpeedAlongHeading > 5)
                    {
                        incAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;
                    }

                    Parameters parameters = new ViewModel.Parameters
                    {
                        Name = response.Type,

                        AirSpeedInd = Math.Round(response.AirspeedInd, 2),
                        GroundSpeed = Math.Round(response.GroundSpeed, 2),
                        CrossWind = Math.Round(response.CrossWind, 2),
                        HeadWind = Math.Round(response.HeadWind, 2),
                        Slip = Math.Round(incAngle, 2),
                        Bounces = stateMachine.Bounces,
                        Latitude = Math.Round(response.Latitude, 2),
                        Longitude = Math.Round(response.Longitude, 2),
                        FPM = FPM,
                        Gforce = Math.Round(gforce, 2),
                        LandingDistance = stateMachine.landingDistance
                    };
                    this.SetParameters(parameters);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //some params are missing. likely the user is in the main menu. ignore
            }
        }

        public void SetParametersFromCVS()
        {
            DataTable dataTable = LandingLogger.GetLandingLogData();
            int mostRecent = 0;

            try
            {
                Parameters parameters = new ViewModel.Parameters
                {
                    // Row[0] is Time
                    Name = (string)dataTable.Rows[mostRecent][1],
                    FPM = (int)((double)dataTable.Rows[mostRecent][2]),
                    LandingDistance = (int)((double)dataTable.Rows[mostRecent][3]),
                    Gforce = (double)dataTable.Rows[mostRecent][4],
                    AirSpeedInd = (double)dataTable.Rows[mostRecent][5],
                    GroundSpeed = (double)dataTable.Rows[mostRecent][6],
                    HeadWind = (double)dataTable.Rows[mostRecent][7],
                    CrossWind = (double)dataTable.Rows[mostRecent][8],
                    Slip = (double)dataTable.Rows[mostRecent][9],
                    Bounces = (int)((double)(double)dataTable.Rows[mostRecent][10])
                };
                this.SetParameters(parameters);
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
