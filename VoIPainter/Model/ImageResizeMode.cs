using System.Collections.Generic;

namespace VoIPainter.Model
{
    

    public class ImageResizeMode
    {
        public enum Mode
        {
            Stretch,
            Crop
        }

        public static Dictionary<Mode, string> ModeNames { get; } = new Dictionary<Mode, string>()
        {
            { Mode.Stretch, Strings.ImageResizeModeStretch },
            { Mode.Crop, Strings.ImageResizeModeCrop }
        };
    }
}
