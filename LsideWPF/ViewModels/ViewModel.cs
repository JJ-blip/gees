namespace LsideWPF.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Forms;
    using LsideWPF.Services;
    using Microsoft.Extensions.DependencyInjection;

    // The LRMDisplayViewModel's viewModel
    public class ViewModel : BindableBase
    {
        private readonly ISimService simService = App.Current.Services.GetService<ISimService>();
        private readonly ISlipLogger sipLogger = App.Current.Services.GetService<ISlipLogger>();

        private bool updatable = false;

        private bool connected = false;

        public ViewModel()
        {
            this.Connected = false;
            this.updatable = false;

            ((INotifyPropertyChanged)this.simService).PropertyChanged += this.Connected_PropertyChanged;
            ((INotifyPropertyChanged)this.sipLogger).PropertyChanged += this.SipCompleted_PropertyChanged;
        }

        /** Main Form Data **/
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

        public bool Connected
        {
            get
            {
                return this.connected;
            }

            set
            {
                this.connected = value;
                this.OnPropertyChanged("ConnectedColor");
                this.OnPropertyChanged("ConnectedString");
            }
        }

        public string ConnectedString
        {
            get
            {
                if (this.Connected)
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
                if (!this.Connected)
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
                return this.updatable;
            }

            set
            {
                this.updatable = value;
                this.OnPropertyChanged();
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

        public bool SlipHasCompleted
        {
            get
            {
                return this.sipLogger.HasCompleted();
            }
        }

        private void SipCompleted_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "HasCompleted":
                    {
                        this.OnPropertyChanged("SlipHasCompleted");
                    }

                    break;
            }
        }

        private void Connected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Connected":
                    {
                        if (this.simService.Connected)
                        {
                            this.Connected = true;
                        }
                        else
                        {
                            this.Connected = false;
                        }

                        break;
                    }
            }
        }
    }
}
