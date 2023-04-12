namespace LsideWPF.Services
{
    using LsideWPF.Services;

    public interface ISlipLogger
    {
        void Add(PlaneInfoResponse response);

        void Reset();

        void WriteLogToFile();
    }
}
