using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LsideWPF.model
{
    public class LogEntry : INotifyPropertyChanged, IEditableObject
    {
        [Name("Time")]
        public DateTime Time { get; set; }
        [Name("Plane")]
        public string Plane { get; set; }
        [Name("FPM")]
        public int Fpm { get; set; }
        [Name("Slowing Distance (ft)")]
        public int SlowingDistance { get; set; }
        [Name("Impact (G)")]
        public double Gforce { get; set; }
        [Name("Air Speed (kt)")]
        public double AirSpeedInd { get; set; }
        [Name("Ground Speed (kt)")]
        public double GroundSpeed { get; set; }
        [Name("Headwind (kt)")]
        public double HeadWind { get; set; }
        [Name("Crosswind (kt)")]
        public double CrossWind { get; set; }
        [Name("SlipAngle (deg)")]
        public double SlipAngle { get; set; }
        [Name("Bounces")]
        public int Bounces { get; set; }
        [Name("Bank Angle (deg)")]
        public double BankAngle { get; set; }
        [Name("Distance From Aim Point (ft)")]
        public int AimPointOffset { get; set; }
        [Name("Offser From Cnt Line (ft)")]
        public int CntLineOffser { get; set; }
        [Name("Airport")]
        public string Airport { get; set; }
        [Name("DriftAngle (deg)")]
        public double DriftAngle { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void BeginEdit()
        {
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            throw new NotImplementedException();
        }

        public void EndEdit()
        {
            throw new NotImplementedException();
        }
    }

    public class LogEntries : ObservableCollection<LogEntry>
    {
        // Creating the Tasks collection in this way enables data binding from XAML.
    }

}
