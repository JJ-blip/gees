using LsideWPF.Common;

namespace LsideWPF.Utils
{
    public interface ISlipLogger
    {
        void Add(PlaneInfoResponse response);
        void WriteLogToFile();
    }
}
