﻿using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;

namespace VoIPainter.Model
{
    public static class Phone
    {
        // Known image dimensions
        private static readonly ScreenInfo SI_G_320_196_80_53 = new ScreenInfo(new Size(320, 196), new Size(80, 53), false);
        private static readonly ScreenInfo SI_C_320_212_80_53 = new ScreenInfo(new Size(320, 212), new Size(80, 53));
        private static readonly ScreenInfo SI_C_240_320_117_117 = new ScreenInfo(new Size(250, 320), new Size(117, 117));
        private static readonly ScreenInfo SI_C_800_480_139_109 = new ScreenInfo(new Size(800, 480), new Size(139, 109));
        private static readonly ScreenInfo SI_C_640_480_123_111 = new ScreenInfo(new Size(640, 480), new Size(123, 111));
        private static readonly ScreenInfo SI_C_272_480_139_109 = new ScreenInfo(new Size(272, 480), new Size(139, 109));   // KEM
        private static readonly ScreenInfo SI_C_320_480_139_109 = new ScreenInfo(new Size(320, 480), new Size(139, 109));   // KEM

        /// <summary>
        /// Specifies known models and their required image dimensions
        /// </summary>
        public static Dictionary<string, ScreenInfo> Models { get; } = new Dictionary<string, ScreenInfo>()
        {
            { "7941", SI_G_320_196_80_53 },
            { "7961", SI_G_320_196_80_53 },
            { "7942", SI_G_320_196_80_53 },
            { "7962", SI_G_320_196_80_53 },
            { "7945", SI_C_320_212_80_53 },
            { "7965", SI_C_320_212_80_53 },
            { "7970", SI_C_320_212_80_53 },
            { "7971", SI_C_320_212_80_53 },
            { "7975", SI_C_320_212_80_53 },
            { "8821", SI_C_240_320_117_117 },
            { "8841", SI_C_800_480_139_109 },
            { "8845", SI_C_800_480_139_109 },
            { "8851", SI_C_800_480_139_109 },
            { "8861", SI_C_800_480_139_109 },
            { "8865", SI_C_800_480_139_109 },
            { "8941", SI_C_640_480_123_111 },
            { "8945", SI_C_640_480_123_111 },
            // KEMs
            //{ "8800 BEKEM", SI_C_272_480_139_109 },
            //{ "8851 BEKEM", SI_C_320_480_139_109 },
            //{ "8861 BEKEM", SI_C_320_480_139_109 },
            //{ "8865 BEKEM", SI_C_320_480_139_109 },
            { "9951", SI_C_640_480_123_111 },
            { "9971", SI_C_640_480_123_111 }
        };

        /// <summary>
        /// Get the length of the ringtone for the specified model
        /// </summary>
        /// <param name="model">Phone model</param>
        /// <returns>Length in samples</returns>
        public static int RingLength(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentNullException(nameof(model));
            if (!System.Text.RegularExpressions.Regex.IsMatch(model, "^\\d{4}$"))
                throw new ArgumentException(Strings.ValidationModel, nameof(model));
            return model.StartsWith("78", StringComparison.InvariantCultureIgnoreCase) || model.StartsWith("88", StringComparison.InvariantCultureIgnoreCase) ? 160_800 : 16_080;
        }

        /// <summary>
        /// Represents a screen geometry
        /// </summary>
        public struct ScreenInfo : IEquatable<ScreenInfo>
        {
            public bool Color { get; }
            public Size ImageSize { get; }
            public Size ThumbnailSize { get; }

            public ScreenInfo(Size imageSize, Size thumbnailSize, bool color = true)
            {
                Color = color;
                ImageSize = imageSize;
                ThumbnailSize = thumbnailSize;                
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ScreenInfo))
                    return false;
                return Equals((ScreenInfo)obj);
            }

            public override int GetHashCode() => (Color ? 1 : 0) ^ ImageSize.GetHashCode() ^ ThumbnailSize.GetHashCode();

            public static bool operator ==(ScreenInfo left, ScreenInfo right) => left.Equals(right);

            public static bool operator !=(ScreenInfo left, ScreenInfo right) => !(left == right);

            public bool Equals(ScreenInfo other) => other.Color == Color && other.ImageSize == ImageSize && other.ThumbnailSize == ThumbnailSize;
        }
    }
}
