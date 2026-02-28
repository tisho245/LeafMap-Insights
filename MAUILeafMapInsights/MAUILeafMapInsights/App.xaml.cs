using System.Diagnostics;

namespace MAUILeafMapInsights
{
    public partial class App : Application
    {
        public App()
        {
            StartupLog.Info("App() start");
            try
            {
                InitializeComponent();
                StartupLog.Info("App() InitializeComponent done");
            }
            catch (Exception ex)
            {
                StartupLog.Error(ex);
                throw;
            }
        }

        public App(IServiceProvider services) : this()
        {
            StartupLog.Info("App(IServiceProvider) setting Services");
            AppServices.Services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            StartupLog.Info("CreateWindow start");
            if (AppServices.Services == null && activationState?.Context?.Services is IServiceProvider sp)
            {
                AppServices.Services = sp;
                StartupLog.Info("CreateWindow: Services set from activationState");
            }

            try
            {
                StartupLog.Info("CreateWindow: creating AppShell");
                var shell = new AppShell();
                StartupLog.Info("CreateWindow: AppShell created");
                return new Window(shell);
            }
            catch (Exception ex)
            {
                StartupLog.Error(ex);
                try
                {
                    var errorPage = new ContentPage
                    {
                        Content = new ScrollView
                        {
                            Content = new VerticalStackLayout
                            {
                                new Label { Text = "Startup error – see log: " + StartupLog.LogPath, FontSize = 18, LineBreakMode = LineBreakMode.WordWrap },
                                new Label { Text = ex.ToString(), LineBreakMode = LineBreakMode.WordWrap, FontSize = 12 }
                            }
                        }
                    };
                    return new Window(errorPage);
                }
                catch (Exception ex2)
                {
                    StartupLog.Error("Error page failed: " + ex2);
                    throw;
                }
            }
        }
    }
}