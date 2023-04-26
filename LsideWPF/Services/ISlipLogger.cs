namespace LsideWPF.Services
{
    using System.Collections.Generic;
    using System.Data;

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

        List<SlipLogEntry> GetListDataTable(DataTable dt);
    }
}
