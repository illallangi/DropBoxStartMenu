namespace Illallangi.DropBox.StartMenu
{
    public sealed class ProgramModule<T, I> : Ninject.Modules.NinjectModule where T: I
    {
        #region Fields

        private readonly string[] currentArguments;

        #endregion

        #region Constructor

        public ProgramModule(string[] arguments)
        {
            this.currentArguments = arguments;
        }

        #endregion

        #region Properties

        private string[] Arguments
        {
            get { return this.currentArguments; }
        }

        #endregion

        #region Methods

        public override void Load()
        {
            Bind<I>()
                .To<T>()
                .InSingletonScope()
                .WithConstructorArgument("arguments", this.Arguments);

            Bind<IConfig>()
                .To<Config>()
                .InSingletonScope();

            Bind<IShortcutSource>()
                .To<ShortcutSource>()
                .InSingletonScope();
        }

        #endregion
    }
}
