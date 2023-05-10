namespace LsideWPF.ViewModels
{
    using System.Collections.Generic;
    using LsideWPF.Common;
    using LsideWPF.Services;
    using Microsoft.Extensions.DependencyInjection;

    public class SlipViewModel : BindableBase
    {
        private readonly ISlipLogger slipLogger = App.Current.Services.GetService<ISlipLogger>();

        public SlipViewModel()
        {
            // default inititilisation - initilise the viewModel via loggers slip acquistion logic
            this.SlipEntries = this.slipLogger.GetLogEntries();
            this.FullFileName = this.slipLogger.GetFullFilename();
            this.HasComplted = this.slipLogger.HasCompleted();
        }

        public List<SlipLogEntry> SlipEntries { get; private set; }

        public string FullFileName { get; private set; }

        public bool HasComplted { get; private set; }

        public string Path
        {
            set
            {
                // re - initilise the viewModel from the data behind the given file.
                // discards any previous inititilisation
                FileService fileService = new FileService();
                var dt = fileService.GetDataTable(value);
                var se = this.slipLogger.GetListDataTable(dt);
                this.SlipEntries = se;
                this.FullFileName = value;
                this.HasComplted = true;

                // alls changed
                this.OnPropertyChanged(string.Empty);
            }
        }
    }
}
