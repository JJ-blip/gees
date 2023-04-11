using LsideWPF.Common;
using LsideWPF.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace LsideWPF.ViewModels
{
    //The LRMDisplayViewModel's viewModel
    public class ViewModel : BindableBase
    {
        private bool updatable = false;
        private ILandingLoggerService landingLogger = App.Current.Services.GetService<ILandingLoggerService>();
        private ISimService simService = App.Current.Services.GetService<ISimService>();

        public ViewModel()
        {
            Connected = false;
            updatable = false;

            ((INotifyPropertyChanged)simService).PropertyChanged += Connected_PropertyChanged;
        }

        private void Connected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Connected":
                    {
                        if (simService.Connected)
                        {
                            Connected = true;
                        }
                        else
                        {
                            Connected = false;
                        }

                        break;
                    }
            }
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
                OnPropertyChanged("ConnectedColor");
                OnPropertyChanged("ConnectedString");
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

        public bool UpdateAvailable
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
    }
}
