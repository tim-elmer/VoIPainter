using System;
using System.Windows;

namespace VoIPainter.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(Control.SettingsController settingsController)
        {
            InitializeComponent();
            DataContext = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
        }
    }
}
