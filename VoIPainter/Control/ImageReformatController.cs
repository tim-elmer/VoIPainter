using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using VoIPainter.Model;
using SixLabors.ImageSharp.Formats.Png;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles resizing and reformatting the image to the needed specs
    /// </summary>
    public class ImageReformatController
    {
        private readonly LogController _logController;

        /// <summary>
        /// The path to the image
        /// </summary>
        public string Path { get; set; }

        public ImageReformatController(LogController logController) => _logController = logController ?? throw new ArgumentNullException(nameof(logController));

        /// <summary>
        /// Format the image
        /// </summary>
        /// <param name="stream">The output stream</param>
        /// <param name="screenInfo">The screen to use</param>
        /// <param name="thumbnail">Is a thumbnail desired</param>
        public void Format(Stream stream, Phone.ScreenInfo screenInfo, bool thumbnail)
        {

            var orig = Image.Load(Path);
            var resizeOptions = new ResizeOptions
            {
                Size = thumbnail ? screenInfo.ThumbnailSize : screenInfo.ImageSize
            };

            switch ((ImageResizeMode.Mode)Enum.Parse(typeof(ImageResizeMode.Mode), Settings.Default.ResizeMode))
            {
                case ImageResizeMode.Mode.Stretch:
                    resizeOptions.Mode = ResizeMode.Stretch;
                    break;
                        
                case ImageResizeMode.Mode.Crop:
                    resizeOptions.Mode = ResizeMode.Crop;
                    break;
            }

            orig.Mutate(i => i.Resize(resizeOptions));

            orig.SaveAsPng(stream, new PngEncoder() { /*BitDepth = SixLabors.ImageSharp.Formats.Png.PngBitDepth.Bit16,*/ ColorType = screenInfo.Color ? PngColorType.Rgb : PngColorType.Grayscale });
            orig.Dispose();
            
        }
    }
}
