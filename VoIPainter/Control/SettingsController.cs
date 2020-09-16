using System;
using System.Collections.Generic;
using System.ComponentModel;
using VoIPainter.Model;
using System.Linq;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles settings (imagine that)
    /// </summary>
    public class SettingsController : INotifyPropertyChanged
    {
        private readonly LogController _logController;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The phone to push to
        /// </summary>
        public string LastTarget
        {
            get => Settings.Default.LastTarget;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationIp));
                    throw new ArgumentNullException(nameof(LastTarget), Strings.ValidationIp);
                }
                Settings.Default.LastTarget = value;
                OnPropertyChanged(nameof(LastTarget));
                Settings.Default.Save();
            }
        }

        public string LastUser
        {
            get => Settings.Default.LastUser;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationUsername));
                    throw new ArgumentNullException(nameof(LastUser), Strings.ValidationUsername);
                }
                Settings.Default.LastUser = value;
                OnPropertyChanged(nameof(LastUser));
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// The phone model
        /// </summary>
        public string LastModel 
        { 
            get => Settings.Default.LastModel; 
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(LastModel), Strings.ValidationModel);

                if (!Phone.Models.ContainsKey(value.ToUpperInvariant().Trim()))
                {
                    _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationModel));
                    throw new ArgumentOutOfRangeException(nameof(LastModel), Strings.ValidationModel);
                }
                Settings.Default.LastModel = value;
                OnPropertyChanged(nameof(LastModel));
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// The resize mode
        /// </summary>
        public ImageResizeMode.Mode ResizeMode
        {
            get => Enum.Parse<ImageResizeMode.Mode>(Settings.Default.ResizeMode);
            set
            {
                if (!Enum.GetNames(typeof(ImageResizeMode.Mode)).Contains(value.ToString()))
                    throw new ArgumentOutOfRangeException(nameof(ResizeMode));
                Settings.Default.ResizeMode = value.ToString();
                OnPropertyChanged(nameof(ResizeMode));
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Translation dictionary for resize modes
        /// </summary>
        public static Dictionary<ImageResizeMode.Mode, string> ImageResizeModeNames => ImageResizeMode.ModeNames;

        /// <summary>
        /// If image contrast should duck automatically
        /// </summary>
        public bool AutoDuckContrast
        {
            get => Settings.Default.AutoDuckContrast;
            set
            {
                Settings.Default.AutoDuckContrast = value;
                OnPropertyChanged(nameof(AutoDuckContrast));
                Settings.Default.Save();
            }
        }
        
        public float TargetContrast
        {
            get => Settings.Default.TargetContrast;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(TargetContrast));
                Settings.Default.TargetContrast = value;
                OnPropertyChanged(nameof(TargetContrast));
                Settings.Default.Save();
            }
        }

        public SettingsController(LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));

            // Borrowed from https://stackoverflow.com/a/2698338
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();

                _logController.Log(new Entry(LogSeverity.Info, Strings.StatusUpgradeSettings));
            }

            if (string.IsNullOrWhiteSpace(LastUser))
                LastUser = Environment.UserName;
        }

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
