namespace LsideWPF.Services
{
    using System.Collections.Generic;

    public interface ISlipLogger
    {
        // Only meaningfull after FinishLogging completion
        List<SlipLogEntry> GetLogEntries();

        // Only meaningfull after FinishLogging completion
        string GetFullFilename();

        bool HasCompleted();

        void BeginLogging();

        void Log(PlaneInfoResponse response);

        void FinishLogging();

        void CancelLogging();
    }
}
