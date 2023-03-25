using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GeesWPF.model
{
    public class LandingViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string planeFilter = "";
        private DataTable logTable;

        public LandingViewModel()
        {
            UpdateTable();
        }

        public DataTable LandingTable
        {
            get
            {
                return logTable;
            }
        }
        public string PlaneFilter
        {
            get
            {
                return planeFilter;
            }
            set
            {
                planeFilter = value;
                logTable.DefaultView.RowFilter = "Plane Like '%" + value + "%'";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LandingTable"));
            }
        }

        public void UpdateTable()
        {
            LandingLogger logger = new LandingLogger();
            logTable = logger.LandingLog;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LandingTable"));
        }

        public void LogParams(StateMachine stateMachine)
        {
            if (stateMachine.landingResponses.Count == 0)
            {
                return;
            }

            PlaneInfoResponse response = stateMachine.landingResponses.First();

            double lr = 60 * response.LandingRate;
            Int32 fpm = Convert.ToInt32(-lr);

            // compute g force, taking largest value
            double gforce = 0;
            foreach (var resp in stateMachine.landingResponses)
            {
                if (resp.Gforce > gforce)
                {
                    gforce = resp.Gforce;
                }
            }

            double incAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;

            LandingLogger logger = new LandingLogger();
            logger.EnterLog(new LandingLogger.LogEntry
            {
                Time = DateTime.Now,
                Plane = response.Type,
                Fpm = fpm,
                Gforce = Math.Round(gforce, 2),
                AirSpeedInd = Math.Round(response.AirspeedInd, 2),
                GroundSpeed = Math.Round(response.GroundSpeed, 2),
                HeadWind = Math.Round(response.HeadWind, 2),
                CrossWind = Math.Round(response.CrossWind, 2),
                Sideslip = Math.Round(incAngle, 2),
                Bounces = stateMachine.Bounces,
                landingDistance = Convert.ToInt32(stateMachine.landingDistance)
            });
            UpdateTable();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        //TODO https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification?view=netframeworkdesktop-4.8

        protected void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
