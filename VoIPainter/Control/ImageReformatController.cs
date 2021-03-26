using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using VoIPainter.Model;
using SixLabors.ImageSharp.Formats.Png;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles resizing and reformatting the image to the needed specs
    /// </summary>
    public class ImageReformatController : INotifyPropertyChanged
    {
        private string _path;
        private readonly MainController _mainController;
        private readonly LogController _logController;
        private Image _original;


        /// <summary>
        /// The path to the image
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                try
                {
                    _original = Image.Load(Path);
                }
                catch (Exception e)
                {
                    _logController.Log(new Entry(LogSeverity.Error, e.Message));
                    return;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
                Format();
            }
        }

        public Image Image { get; private set; }
        public Image Thumbnail { get; private set; }

        public ImageSource ImagePreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Path))
                    return null;

                // Via https://stackoverflow.com/a/41999179

                using var memoryStream = new MemoryStream();
                Image.Save(memoryStream, PngEncoder);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memoryStream;
                image.EndInit();
                return image;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private PngEncoder PngEncoder => new PngEncoder()
        {
            ColorType = Phone.Models[_mainController.SettingsController.LastModel].Color ? PngColorType.Rgb : PngColorType.Grayscale
        };

        public ImageReformatController(MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));

            _mainController.SettingsController.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SettingsController.LastModel):
                    case nameof(SettingsController.ResizeMode):
                    case nameof(SettingsController.TargetContrast):
                        Format();
                        break;
                }
            };

            _logController = _mainController.LogController;
        }

        /// <summary>
        /// Format the image
        /// </summary>
        public void Format()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return;

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusFormattingImage));

            var contrast = Contrast(_original);
            using var working = _original.CloneAs< SixLabors.ImageSharp.PixelFormats.Rgb24>();

            if (contrast > _mainController.SettingsController.TargetContrast)
            {
                if (_mainController.SettingsController.AutoDuckContrast)
                {
                    Duck(working, contrast);
                    _logController.Log(new Entry(LogSeverity.Info, Strings.AutoDuckedContrast));
                }
                else if (_mainController.SettingsController.UseContrastBox)
                {
                    ContrastBox(working, contrast);
                    _logController.Log(new Entry(LogSeverity.Info, Strings.ContrastBox));
                }
                else if (_mainController.MainWindow.ShowMessageBox(Strings.MessageBoxWarnContrastText, Strings.MessageBoxWarnContrastCaption, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, System.Windows.MessageBoxResult.Yes, System.Windows.MessageBoxOptions.None) == System.Windows.MessageBoxResult.Yes)
                    Duck(working, contrast);
                else
                    _logController.Log(new Entry(LogSeverity.Warning, Strings.WarnImageContrast));
            }

            var imageResizeOptions = new ResizeOptions
            {
                Size = Phone.Models[_mainController.SettingsController.LastModel].ImageSize
            };
            var thumbnailResizeOptions = new ResizeOptions
            {
                Size = Phone.Models[_mainController.SettingsController.LastModel].ThumbnailSize
            };

            switch ((ImageResizeMode.Mode)Enum.Parse(typeof(ImageResizeMode.Mode), Settings.Default.ResizeMode))
            {
                case ImageResizeMode.Mode.Stretch:
                    imageResizeOptions.Mode = thumbnailResizeOptions.Mode = ResizeMode.Stretch;
                    break;

                case ImageResizeMode.Mode.Crop:
                    imageResizeOptions.Mode = thumbnailResizeOptions.Mode = ResizeMode.Crop;
                    break;

                case ImageResizeMode.Mode.Center:
                    imageResizeOptions.Mode = thumbnailResizeOptions.Mode = ResizeMode.BoxPad;
                    break;
            }

            if (!(Image is null))
                Image.Dispose();
            if (!(Thumbnail is null))
                Thumbnail.Dispose();
            Image = working.Clone(i => i.Resize(imageResizeOptions));
            Thumbnail = working.Clone(i => i.Resize(thumbnailResizeOptions));

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusFormattingImageDone));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImagePreview)));
        }

        public void Stream(Stream stream, bool thumbnail = false)
        {
            if (thumbnail)
                Thumbnail.SaveAsPng(stream, PngEncoder);
            else
                Image.SaveAsPng(stream, PngEncoder);
        }

        private void Duck(Image orig, float contrast) => orig.Mutate(i => i.Contrast(_mainController.SettingsController.TargetContrast / contrast).Brightness(DeltaBrightness(contrast)));

        private void ContrastBox(Image orig, float contrast)
        {
            Rectangle rectangle = new Rectangle(new Point(0, 0), new Size((int)(orig.Width * 0.445f), orig.Height));
            orig.Mutate(i => i.Contrast(_mainController.SettingsController.TargetContrast / contrast, rectangle).Brightness(DeltaBrightness(contrast), rectangle));
        }

        private float DeltaBrightness(float contrast) => MathF.Max(MathF.Min(MathF.Log(contrast + 1), 0.25f) / _mainController.SettingsController.TargetContrast, 1);

        /// <summary>
        /// Determine the contrast of an image.
        /// </summary>
        /// <param name="image">Image to analyze</param>
        /// <returns>RMS contrast</returns>
        /// <remarks>Uses RMS: sqrt(1 / (M * N) * sum(N - 1, i = 0, sum(M - 1, j = 0, ((I_ij - mean(I)) / mean(I)) ^ 2)))</remarks>
        private static float Contrast(Image image)
        {
            var totalIntensity = 0f;

            using var small = image.CloneAs<SixLabors.ImageSharp.PixelFormats.Rgb24>();
            
            // Resize for speed
            small.Mutate(i => i.Resize(100, 100));
            for (int y = 0; y < small.Height; y++)
            {
                var span = small.GetPixelRowSpan(y);
                for (int x = 0; x < span.Length; x++)
                    totalIntensity += (span[x].R / 255f + span[x].G / 255f + span[x].B / 255f) / 3f;
            }

            var avgIntensity = totalIntensity / (small.Height * small.Width);

            var sum = 0f;

            for (int y = 0; y < small.Height; y++)
            {
                var span = small.GetPixelRowSpan(y);
                for (int x = 0; x < span.Length; x++)
                    sum += MathF.Pow((((span[x].R / 255f + span[x].G / 255f + span[x].B / 255f) / 3f) - avgIntensity) / avgIntensity, 2f);
            }

            return MathF.Sqrt(sum / (small.Height * small.Width));
        }
    }
}
