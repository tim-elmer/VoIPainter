using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VoIPainter.Model;
using VoIPainter.Model.Logging;

namespace VoIPainter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Control.MainController _mainController;
        private readonly Control.LogController _logController;

        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();


        public ObservableCollection<Tuple<string, string, string>> LogEntries { get; private set; } = new ObservableCollection<Tuple<string, string, string>>();

        public string Error { get; private set; }

        public MainWindow(Control.MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));
            _logController = _mainController.LogController;

            InitializeComponent();
            DataContext = _mainController.SettingsController;
            PhoneModelComboBox.ItemsSource = Phone.Models;
            LogListBox.ItemsSource = _logController.LogEntries;
            ImageHost.DataContext = _mainController.ImageReformatController;

            _logController.LogEntries.CollectionChanged += (s, e) => Dispatcher.Invoke(() => LogListBox.ScrollIntoView(_mainController.LogController.LogEntries.Last()));

            _mainController.ImageServerController.Run();

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusReady));
            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            _ = _mainController.UpdateCheckController.GetUpdateAvailable().ContinueWith((task) =>
              {

                  var response = task.Result;
                  if (response is null)
                      return;

                  Dispatcher.Invoke(() =>
                  {
                      switch (MessageBox.Show(string.Format(CultureInfo.InvariantCulture, Strings.MessageBoxUpdateAvailableText, response.TagName, response.PublishedAt.ToLocalTime(), response.Body), Strings.MessageBoxUpdateAvailableCaption, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes))
                      {
                          case MessageBoxResult.Yes:
                              UICommon.OpenLink(response.HtmlUrl);
                              break;
                          case MessageBoxResult.No:
                              break;
                      }
                  });
              }, TaskScheduler.Default);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e) => Browse();

        private void ApplyButton_Click(object sender, RoutedEventArgs e) => Apply();

        private void Browse()
        {
            if (_openFileDialog.ShowDialog().GetValueOrDefault())
                _mainController.ImageReformatController.Path = _openFileDialog.FileName;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "General exceptions caught for user feedback")]
        private async void Apply()
        {            
            
            try
            {                
                await _mainController.RequestController.Send(passwordBox.Password).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logController.Log(new Entry(LogSeverity.Error, e.Message));
            }
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusClosing));
            _mainController.ImageServerController.Stop();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e) => new AboutWindow().ShowDialog();

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e) => UICommon.OpenLink(new Uri("https://github.com/tim-elmer/VoIPainter/wiki"));

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) => new SettingsWindow(_mainController.SettingsController).ShowDialog();

        private void CheckForUpdatesMenuItem_Click(object sender, RoutedEventArgs e) => CheckForUpdates();
    }
}
