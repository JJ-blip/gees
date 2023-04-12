namespace LsideWPF
{
    using Ninject.Modules;

    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            // Bind<IMailSender>().To<MockMailSender>();
        }
    }
}
