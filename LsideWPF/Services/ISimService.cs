namespace LsideWPF.Services
{
    public interface ISimService
    {
        bool Connected
        {
            get;
        }

        bool Crashed
        {
            get;
        }
    }
}
