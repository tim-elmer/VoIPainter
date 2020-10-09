using System;
using System.Windows;

namespace VoIPainter.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly Control.SettingsController _settingsController;

        public SettingsWindow(Control.SettingsController settingsController)
        {
            InitializeComponent();
            DataContext = _settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
        }
    }
}
