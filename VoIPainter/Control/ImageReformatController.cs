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
        private LogController _logController;

        /// <summary>
        /// The path to the image
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
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
                throw new ArgumentNullException(nameof(Path), Strings.ValidationImage);

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusFormattingImage));

            var orig = Image.Load(Path);

            var contrast = Contrast(orig);

            if (contrast > .6f)
                _logController.Log(new Entry(LogSeverity.Warning, Strings.WarnImageContrast));

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

            Image = orig.Clone(i => i.Resize(imageResizeOptions));
            Thumbnail = orig.Clone(i => i.Resize(thumbnailResizeOptions));

            orig.Dispose();

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

            var avgIntensity = totalIntensity / 10_000f;

            var sum = 0f;

            for (int y = 0; y < small.Height; y++)
            {
                var span = small.GetPixelRowSpan(y);
                for (int x = 0; x < span.Length; x++)
                    sum += MathF.Pow((((span[x].R / 255f + span[x].G / 255f + span[x].B / 255f) / 3f) - avgIntensity) / avgIntensity, 2f);
            }

            return MathF.Sqrt(sum / 10_000f);
        }
    }
}
