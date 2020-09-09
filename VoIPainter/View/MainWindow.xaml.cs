using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using VoIPainter.Model;
using VoIPainter.Model.Logging;

namespace VoIPainter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly Control.ImageServerController _imageServerController;
        private readonly Control.LogController _logController = new Control.LogController();
        private readonly Control.RequestController _requestController;
        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();
        private string _target;
        private string _username;
        private string _image;
        private string _model;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The phone to push to
        /// </summary>
        public string Target
        {
            get => _target;
            set
            {
                _target = Settings.Default.LastTarget = value;
                OnPropertyChanged(nameof(Target));
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = Settings.Default.LastUser = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        /// <summary>
        /// The path to the image
        /// </summary>
        public string Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        /// <summary>
        /// The model of phone to operate on
        /// </summary>
        public string Model
        {
            get => _model;
            set
            {
                _model = Settings.Default.LastModel = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        public ObservableCollection<Tuple<string, string, string>> LogEntries { get; private set; } = new ObservableCollection<Tuple<string, string, string>>();

        public string Error { get; private set; }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Username):
                        if (string.IsNullOrWhiteSpace(Username))
                            return Strings.ValidationUsername;
                        break;
                    case nameof(Target):
                        if (!System.Net.IPAddress.TryParse(Target, out _))
                            return Strings.ValidationIp;
                        break;
                    default:
                        break;
                }
                return null;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Target = Settings.Default.LastTarget;
            Username = Settings.Default.LastUser;
            Model = Settings.Default.LastModel;
            DataContext = this;
            PhoneModelComboBox.ItemsSource = Phone.Models;
            LogListBox.ItemsSource = _logController.LogEntries;

            _logController.LogEntries.CollectionChanged += (s, e) => Dispatcher.Invoke(() => LogListBox.ScrollIntoView(_logController.LogEntries.Last()));

            _requestController = new Control.RequestController(_logController);
            _imageServerController = new Control.ImageServerController(_logController);

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusReady));
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e) => Browse();

        private void ApplyButton_Click(object sender, RoutedEventArgs e) => Apply();

        private void Browse()
        {
            if (_openFileDialog.ShowDialog().GetValueOrDefault())
                Image = _openFileDialog.FileName;
        }

        private void Apply()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationUsername));
                return;
            }
            if (string.IsNullOrWhiteSpace(Target))
            {
                _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationIp));
                return;
            }
            if (passwordBox.SecurePassword.Length <= 0)
            {
                _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationPassword));
                return;
            }
            if (string.IsNullOrWhiteSpace(Image))
            {
                _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationImage));
                return;
            }
            if (string.IsNullOrWhiteSpace(Model))
            {
                _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationModel));
                return;
            }

            _imageServerController.ScreenInfo = Phone.Models[Model];

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusFormattingImage));
            _imageServerController.Image = Image;
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusImageServerStarting));
            try
            {
                _imageServerController.Run();
                _logController.Log(new Entry(LogSeverity.Info, Strings.StatusPhoneSendingCommand));
                _requestController.Send(Target, Username, passwordBox.Password);
            }
            catch (Exception e)
            {
                _logController.Log(new Entry(LogSeverity.Error, e.Message));
            }
            
        }

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _imageServerController.Stop();
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusCanceled));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusClosing));
            _imageServerController.Stop();
            Settings.Default.Save();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e) => new AboutWindow().ShowDialog();

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e) => AboutWindow.OpenLink("https://github.com/tim-elmer/VoIPainter/wiki");

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) => new SettingsWindow().ShowDialog();
    }
}
