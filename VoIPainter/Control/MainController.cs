using System;

namespace VoIPainter.Control
{
    /// <summary>
    /// Main control class
    /// </summary>
    public class MainController
    {
        private static MainController _instance;

        public SettingsController SettingsController { get; }

        public LogController LogController { get; } = new LogController();
        public ImageServerController ImageServerController { get; }
        public RequestController RequestController { get; }
        public UpdateCheckController UpdateCheckController { get; }
        public ImageReformatController ImageReformatController { get; }
        public View.MainWindow MainWindow { get; }

        public MainController()
        {
            if (!(_instance is null))
                throw new InvalidOperationException();
            _instance = this;

            SettingsController = new SettingsController(LogController);
            ImageReformatController = new ImageReformatController(this);
            ImageServerController = new ImageServerController(this);
            RequestController = new RequestController(this);
            UpdateCheckController = new UpdateCheckController(LogController);
            MainWindow = new View.MainWindow(this);
            MainWindow.Show();
        }
    }
}
