using System;
using System.Windows;

namespace VoIPainter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Borrowed from https://stackoverflow.com/a/2698338
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            StartupUri = new Uri("View/MainWindow.xaml", UriKind.Relative);
        }
    }
}
