namespace MAUILeafMapInsights
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Прихващаме необработени изключения – при краш без предупреждение поне да се изпише грешката.
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                System.Diagnostics.Debug.WriteLine($"UnhandledException: {ex}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
                        page?.DisplayAlert("Грешка", ex.ToString(), "OK");
                    }
                    catch { }
                });
            };
            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"UnobservedTaskException: {e.Exception}");
                e.SetObserved();
            };
        }

        /// <summary>При създаване на прозореца – AppServices.Services вече е зададен в MauiProgram след Build().</summary>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                if (AppServices.Services == null && activationState?.Context?.Services is IServiceProvider sp)
                    AppServices.Services = sp;
                if (AppServices.Services == null)
                    System.Diagnostics.Debug.WriteLine("MAUI CreateWindow: AppServices.Services is still null – DI may fail.");
                return new Window(new AppShell());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateWindow failed: {ex}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
                        page?.DisplayAlert("Грешка при старт", ex.Message, "OK");
                    }
                    catch { }
                });
                throw;
            }
        }
    }
}