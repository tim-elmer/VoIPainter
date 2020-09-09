using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using VoIPainter.Model;

namespace VoIPainter.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private ImageResizeMode.Mode _imageResizeMode;
        public ImageResizeMode.Mode ImageResizeMode
        {
            get => _imageResizeMode;
            set
            {
                _imageResizeMode = value;
                Settings.Default.ResizeMode = _imageResizeMode.ToString();
                Settings.Default.Save();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageResizeMode)));
            }
        }

        public Dictionary<ImageResizeMode.Mode, string> ImageResizeModeNames => Model.ImageResizeMode.ModeNames;

        public SettingsWindow()
        {
            ImageResizeMode = (ImageResizeMode.Mode)Enum.Parse(typeof(ImageResizeMode.Mode), Settings.Default.ResizeMode);

            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
