namespace LsideWPF.Utils
{
    using LsideWPF.Common;

    public interface ISlipLogger
    {
        void Add(PlaneInfoResponse response);

        void WriteLogToFile();
    }
}
