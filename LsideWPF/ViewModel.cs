using LsideWPF.model;
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
            public double SlipAngle { get; set; }
            public double CrossWind { get; set; }
            public int Bounces { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int SlowingDistance { get; set; }
            public double BankAngle { get; set; }
            public string Airport { get; set; }
            public int AimPointOffset { get; set; }
            public int CntLineOffser { get; set; }
            public double RelativeWindVelocityBodyX {  get; set; }
            public double RelativeWindVelocityBodyZ { get; set; }
            public double DriftAngle { get; set; }

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
            DriftAngle = 0,
            Bounces = 0,
            SlowingDistance = 0,
            BankAngle = 0,
            Airport = "",
            AimPointOffset = 0,
            CntLineOffser = 0,
            Latitude = 0,
            Longitude = 0,
            SlipAngle = 0,
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

        #endregion


        #region Viewmodel Binding properties

        public string StoppingDistText
        {
            get { return _parameters.SlowingDistance.ToString("0 m"); }
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
                    return _parameters.AimPointOffset.ToString("0 m beyond; 0 m short; 0 m bang on!"); 
            } 
        }
        
        public string DistFromCntLine
        {
            get 
            {
                if (_parameters.Airport == "")
                    return "";
                else
                    return _parameters.CntLineOffser.ToString("0 m right; 0 m left ; 0 m bang on!"); 
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
                    double driftAngle = 0;
                    if (response.SpeedAlongHeading > 5)
                    {
                        driftAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;
                    }

                    double slipAngle = Math.Atan(response.RelativeWindVelocityBodyX / response.RelativeWindVelocityBodyZ) * 180 / Math.PI;

                    Parameters parameters = new Parameters
                    {
                        Name = response.Type,

                        AirSpeedInd = Math.Round(response.AirspeedInd, 1),
                        GroundSpeed = Math.Round(response.GroundSpeed, 1),
                        CrossWind = Math.Round(response.CrossWind, 1),
                        // A positive velocity is defined to be toward the tail
                        HeadWind = - Math.Round(response.HeadWind, 1),
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
                Parameters parameters = new Parameters
                {
                    // Row[0] is Time
                    Name = (string)dataTable.Rows[mostRecent][1],
                    FPM = (int)((double)dataTable.Rows[mostRecent][2]),
                    SlowingDistance = (int)((double)dataTable.Rows[mostRecent][3]),
                    Gforce = (double)dataTable.Rows[mostRecent][4],
                    AirSpeedInd = (double)dataTable.Rows[mostRecent][5],
                    GroundSpeed = (double)dataTable.Rows[mostRecent][6],
                    HeadWind = (double)dataTable.Rows[mostRecent][7],
                    CrossWind = (double)dataTable.Rows[mostRecent][8],
                    SlipAngle = (double)dataTable.Rows[mostRecent][9],
                    Bounces = (int)((double)dataTable.Rows[mostRecent][10]),

                    BankAngle = (double)dataTable.Rows[mostRecent][11],
                    AimPointOffset = (int)((double)dataTable.Rows[mostRecent][12]),
                    CntLineOffser = (int)((double)dataTable.Rows[mostRecent][13]),
                    Airport = (string)dataTable.Rows[mostRecent][14],
                    DriftAngle = (double)dataTable.Rows[mostRecent][15],
                    // RelativeWindVelocityBodyX = (double)dataTable.Rows[mostRecent][15],
                    // RelativeWindVelocityBodyZ = (double)dataTable.Rows[mostRecent][16]
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
