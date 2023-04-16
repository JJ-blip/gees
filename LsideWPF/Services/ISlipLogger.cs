namespace LsideWPF.Services
{
    public interface ISlipLogger
    {
        void BeginLogging();

        void Log(PlaneInfoResponse response);

        void FinishLogging();

        void CancelLogging();
    }
}
