using Microsoft.Win32;
using NAudio.Wave;
using System;
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
    public partial class MainWindow : Window, IDisposable
    {
        private readonly Control.MainController _mainController;
        private readonly Control.LogController _logController;
        private readonly OpenFileDialog _imageOpenFileDialog = new OpenFileDialog()
        {
            CheckFileExists = true,
            CheckPathExists = true,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Multiselect = false,
            Title = Strings.TitleSelectImage
        };
        private readonly OpenFileDialog _ringtoneOpenFileDialog = new OpenFileDialog()
        {
            CheckFileExists = true,
            CheckPathExists = true,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            Multiselect = false,
            Title = Strings.TitleSelectRingtone
        };
        private readonly WaveOutEvent _waveOutEvent = new WaveOutEvent
        {
            DeviceNumber = -1
        };

        public MainWindow(Control.MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));
            _logController = _mainController.LogController;

            InitializeComponent();
            DataContext = _mainController.SettingsController;
            PhoneModelComboBox.ItemsSource = Phone.Models;
            LogListBox.ItemsSource = _logController.LogEntries;
            ImageHost.DataContext = _mainController.ImageReformatController;
            RingtonePathTextBlock.DataContext = _mainController.RingtoneReformatController;
            //_ringtoneOpenFileDialog.Filter = _mainController.FileFilter.AudioFilter;
            //_imageOpenFileDialog.Filter = _mainController.FileFilter.ImageFilter;

            _logController.LogEntries.CollectionChanged += (s, e) => Dispatcher.Invoke(() => LogListBox.ScrollIntoView(_mainController.LogController.LogEntries.Last()));

            _mainController.RingtoneReformatController.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName.Equals(nameof(Control.RingtoneReformatController.Ringtone), StringComparison.Ordinal))
                    _waveOutEvent.Stop();
            };

            _mainController.ImageServerController.Run();

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusReady));
            CheckForUpdates();
        }

        /// <summary>
        /// Show a message box. This is mostly a pass-thru for non UI elements that need one.
        /// </summary>
        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options = MessageBoxOptions.None) => MessageBox.Show(this, text, caption, button, icon, defaultResult, options);

        private void CheckForUpdates()
        {
            _ = _mainController.UpdateCheckController.GetUpdateAvailable().ContinueWith((task) =>
              {

                  var response = task.Result;
                  if (response is null)
                      return;

                  Dispatcher.Invoke(() =>
                  {
                      switch (ShowMessageBox(string.Format(CultureInfo.InvariantCulture, Strings.MessageBoxUpdateAvailableText, response.TagName, response.PublishedAt.ToLocalTime(), response.Body), Strings.MessageBoxUpdateAvailableCaption, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes))
                      {
                          case MessageBoxResult.Yes:
                              UICommon.OpenLink(response.HtmlUrl.AbsoluteUri);
                              break;
                          case MessageBoxResult.No:
                              break;
                      }
                  });
              }, TaskScheduler.Default);
        }

        private void BrowseImageButton_Click(object sender, RoutedEventArgs e) => Browse(RequestMode.Background, _imageOpenFileDialog);
        private void BrowseToneButton_Click(object sender, RoutedEventArgs e) => Browse(RequestMode.Ringtone, _ringtoneOpenFileDialog);

        private void ApplyButton_Click(object sender, RoutedEventArgs e) => Apply();

        private void Browse(RequestMode mode, OpenFileDialog dialog)
        {
            if (dialog.ShowDialog().GetValueOrDefault())
                switch (mode)
                {
                    case RequestMode.Background:
                        _mainController.ImageReformatController.Path = dialog.FileName;
                        break;
                    case RequestMode.Ringtone:
                        _mainController.RingtoneReformatController.Path = dialog.FileName;
                        break;
                }
        }

        private async void Apply()
        {            
            
            try
            {
                if (!(_mainController.ImageReformatController.Image is null))
                    await _mainController.RequestController.Send(passwordBox.Password, RequestMode.Background).ConfigureAwait(false);
                if (!(_mainController.RingtoneReformatController.Ringtone is null))
                    await _mainController.RequestController.Send(passwordBox.Password, RequestMode.Ringtone).ConfigureAwait(false);
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

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e) => new MdWindow("ABOUT.md").ShowDialog();

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e) => new MdWindow("README.md").Show();

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) => new SettingsWindow(_mainController.SettingsController).ShowDialog();

        private void CheckForUpdatesMenuItem_Click(object sender, RoutedEventArgs e) => CheckForUpdates();

        private void PlayRingtoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(_waveOutEvent is null))
                _waveOutEvent.Stop();
            if (_mainController.RingtoneReformatController.Ringtone is null)
                return;

            _mainController.RingtoneReformatController.Ringtone.Position = 0;
            _waveOutEvent.Init(new RawSourceWaveStream(_mainController.RingtoneReformatController.Ringtone, Control.RingtoneReformatController.WAVE_FORMAT));
            _waveOutEvent.Play();
        }

        protected virtual void Dispose(bool disposeManaged)
        {
            if (!(_waveOutEvent is null))
                _waveOutEvent.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
