namespace MAUILeafMapInsights
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        /// <summary>При създаване на прозореца задаваме AppServices.Services от контекста – за GetRequired от страниците.</summary>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (activationState?.Context?.Services is IServiceProvider sp)
                AppServices.Services = sp;
            return new Window(new AppShell());
        }
    }
}