using LsideWPF.Common;
using LsideWPF.Models;
using LsideWPF.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Linq;

namespace LsideWPF.ViewModels
{
    public class LandingViewModel : BindableBase
    {
        private ILandingLoggerService landingLogger = App.Current.Services.GetService<ILandingLoggerService>();
        private string planeFilter = "";

        public LandingViewModel()
        {
            // update data
            LogEntries = landingLogger.GetLandingLogEntries();
        }

        public LogEntryCollection LogEntries { get; set; }

        public string PlaneFilter
        {
            get
            {
                return planeFilter;
            }
            set
            {
                planeFilter = value;
                var filtered = LogEntries.Where(entry => entry.Plane.Contains(value));
                LogEntries = new LogEntryCollection
                {
                    filtered
                };

                OnPropertyChanged(nameof(LogEntries));
            }
        }
    }
}
