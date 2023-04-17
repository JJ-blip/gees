namespace LsideWPF.ViewModels
{
    using System.Collections.Generic;
    using LsideWPF.Services;
    using Microsoft.Extensions.DependencyInjection;

    public class SlipViewModel
    {
        private readonly ISlipLogger slipLogger = App.Current.Services.GetService<ISlipLogger>();

        public SlipViewModel()
        {
            this.SlipEntries = this.slipLogger.GetLogEntries();
            this.FullFileName = this.slipLogger.GetFullFilename();
            this.HasComplted = this.slipLogger.HasCompleted();
        }

        public List<SlipLogEntry> SlipEntries { get; }

        public string FullFileName { get; }

        public bool HasComplted { get; }
    }
}
