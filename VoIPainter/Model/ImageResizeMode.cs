﻿using System.Collections.Generic;

namespace VoIPainter.Model
{
    public static class ImageResizeMode
    {
        public enum Mode
        {
            Stretch,
            Crop,
            Center
        }

        public static Dictionary<Mode, string> ModeNames { get; } = new Dictionary<Mode, string>()
        {
            { Mode.Stretch, Strings.ImageResizeModeStretch },
            { Mode.Crop, Strings.ImageResizeModeCrop },
            { Mode.Center, Strings.ImageResizeModeCenter }
        };
    }
}
