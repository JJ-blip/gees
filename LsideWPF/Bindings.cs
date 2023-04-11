using Ninject.Modules;

namespace LsideWPF
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            //Bind<IMailSender>().To<MockMailSender>();
        }
    }
}
