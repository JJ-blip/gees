namespace LsideWPF.ViewModels
{
    using System.Data;
    using System.Linq;
    using LsideWPF.Services;
    using Microsoft.Extensions.DependencyInjection;

    public class LandingViewModel : BindableBase
    {
        private readonly ILandingLoggerService landingLogger = App.Current.Services.GetService<ILandingLoggerService>();
        private string planeFilter = string.Empty;

        public LandingViewModel()
        {
            // update data
            this.LogEntries = this.landingLogger.GetLandingLogEntries();
        }

        public LogEntryCollection LogEntries { get; set; }

        public string PlaneFilter
        {
            get
            {
                return this.planeFilter;
            }

            set
            {
                this.planeFilter = value;
                var filtered = this.LogEntries.Where(entry => entry.Plane.Contains(value));
                this.LogEntries = new LogEntryCollection
                {
                    filtered,
                };

                this.OnPropertyChanged(nameof(this.LogEntries));
            }
        }
    }
}
