using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LsideWPF.model
{
    public class LandingViewModel
    {
        // will be subscribed upon within Window
        public event PropertyChangedEventHandler PropertyChanged;

        private string planeFilter = "";
        private List<LogEntry> logEntries;

        public LandingViewModel()
        {
            UpdateTable();
        }

        public List<LogEntry> LogEntries
        {
            get
            {
                return logEntries;
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
                logEntries = logEntries.Where(entry => entry.Plane.Contains(value)).ToList();
               // logTable.DefaultView.RowFilter = "Plane Like '%" + value + "%'";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LogEntries"));
            }
        }

        public void UpdateTable()
        {
            LandingLogger logger = new LandingLogger();
            logEntries = LandingLogger.GetLandingLogEntries();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LogEntries"));

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

            // angle between heading & ground track
            double driftAngle = Math.Atan(response.LateralSpeed / response.SpeedAlongHeading) * 180 / Math.PI;
            // angle between 
            double slipAngle = Math.Atan(response.CrossWind / response.HeadWind) * 180 / Math.PI;

            LandingLogger logger = new LandingLogger();
            logger.EnterLog(new LogEntry
            {
                Time = DateTime.Now,
                Plane = response.Type,
                Fpm = fpm,
                Gforce = Math.Round(gforce, 1),
                AirSpeedInd = Math.Round(response.AirspeedInd, 1),
                GroundSpeed = Math.Round(response.GroundSpeed, 1),
                HeadWind = Math.Round(response.HeadWind, 1),
                CrossWind = Math.Round(response.CrossWind, 1),
                SlipAngle = Math.Round(slipAngle, 1),
                Bounces = stateMachine.Bounces,
                SlowingDistance = Convert.ToInt32(Math.Truncate(stateMachine.SlowingDistance)),
                AimPointOffset = Convert.ToInt32(Math.Truncate(response.AtcRunwayTdpointRelativePositionZ)),
                CntLineOffser  = Convert.ToInt32(Math.Truncate(response.AtcRunwayTdpointRelativePositionX)),
                BankAngle = Math.Round(response.PlaneBankDegrees, 1),
                Airport = response.AtcRunwayAirportName,
                DriftAngle = Math.Round(driftAngle, 1),
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
