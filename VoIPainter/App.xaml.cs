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
            _ = new Control.MainController();
        }
    }
}
