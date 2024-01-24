using System;
using System.IO;
using System.Threading;
using System.Windows;
using RestoreWindowPlace;

namespace CSharpWpfYouTube
{
    public partial class App : Application
    {
        // static to be across app instances
        private static Mutex? _InstanceMutex;
        private WindowPlace? _windowPlace;

        public static string AppName => "CSharpWpfYouTube";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check that only one app instance can run 
            string error;
            if (!CheckInstanceRunningStatus(out error))
            {
                MessageBox.Show(error, App.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            try
            {
                // If dbFilePath doesn't exist, it will be created with default data
                string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{App.AppName}.sqlite");
                var mainWindow = new MainWindow(new MainViewModel(dbFilePath));
                SetupRestoreWindowPlace(mainWindow);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{App.AppName} will exit on error");
                Current?.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                _windowPlace?.Save();
            }
            catch (Exception)
            {
            }
        }

        private static bool CheckInstanceRunningStatus(out string error)
        {
            error = string.Empty;
#if DEBUG
            return true;
#else            
            _InstanceMutex = new Mutex(false, $"{App.AppName}ClientMutex");
            bool owned = _InstanceMutex.WaitOne(TimeSpan.Zero, false);
            if (!owned)
            {
                error = $"{App.AppName} is already running. Only one instance is allowed to run at a time.";
            }
            return owned;
#endif
        }

        private void SetupRestoreWindowPlace(MainWindow mainWindow)
        {
            string windowPlaceConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{App.AppName}WindowPlace.config");
            _windowPlace = new WindowPlace(windowPlaceConfigFilePath);
            _windowPlace.Register(mainWindow);
        }
    }
}