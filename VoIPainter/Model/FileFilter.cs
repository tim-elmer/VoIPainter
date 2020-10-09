// Currently unused

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Globalization;

//namespace VoIPainter.Model
//{
//    public class FileFilter
//    {
//        private static readonly string[]
//        #region Audio Formats
//            RAW = new string[] { "*.f32", "*.f64", "*.s8", "*.s16", "*.s24", "*.s32", "*.u8", "*.u16", "*.u24", "*.u32", "*.ul", "*.al", "*.lu", "*.la", "*.f4", "*.f8", "*.s1", "*.s2", "*.s3", "*.s4", "*.u2", "*.u3", "*.u4", "*.sb", "*.sw", "*.sl", "*.ub", "*.uw", "*.fssd", "*.sou" },
//            AMIGA = new string[] { "*.8svx" },
//            AIFF = new string[] { "*.aiff", "*.aif", "*.aiffc", "*.aifc" },
//            AMB = new string[] { "*.amb" },
//            SUN = new string[] { "*.au", "*.snd" },
//            AVR = new string[] { "*.avr" },
//            RED_CD = new string[] { "*.cdda", "*.cdr" },
//            CVSDM = new string[] { "*.cvsd", "*.cvs", "*.cvsu" },
//            DATA = new string[] { "*.dat" },
//            DE_VM = new string[] { "*.dvms", "*.vms" },
//            AMR = new string[] { "*.amr-nb", "*.amr-wb" },
//            APPLE_CORE = new string[] { "*.caf" },
//            PARIS = new string[] { "*.paf", "*.fap" },
//            FLAC = new string[] { "*.flac" },
//            GRANDSTREAM = new string[] { "*.gsrt" },
//            GSM = new string[] { "*.gsm" },
//            HCOM = new string[] { "*.hcom" },
//            HTK = new string[] { "*.htk" },
//            IRCAM = new string[] { "*.sf", "*.ircam" },
//            IMA = new string[] { "*.ima" },
//            LPC = new string[] { "*.lpc", "*.lpc10" },
//            MATLAB = new string[] { "*.mat", "*.mat4", "*.mat5" },
//            MACROSYS = new string[] { "*.maud" },
//            SPHERE = new string[] { "*.sph", "*.nist" },
//            OGG = new string[] { "*.ogg", "*.vorbis" },
//            OPUS = new string[] { "*.opus" },
//            PSION = new string[] { "*.prc", "*.wve" },
//            PORTABLE_VOICE = new string[] { "*.pvf" },
//            SOUND_DESIGN = new string[] { "*.sd2" },
//            MIDI = new string[] { "*.sds" },
//            ASTERISK = new string[] { "*.sln" },
//            TURTLE_BEACH = new string[] { "*.smp" },
//            SOUNDER = new string[] { "*.sndr", "*.snd" },
//            SOUND_TOOL = new string[] { "*.sndt", "*.snd" },
//            SOX = new string[] { "*.sox" },
//            YAMAHA = new string[] { "*.txw" },
//            SOUND_BLASTER = new string[] { "*.voc" },
//            DIALOGIC = new string[] { "*.vox" },
//            SONIC_FOUNDRY = new string[] { "*.w64" },
//            WAV = new string[] { "*.wav", "*.wavpcm" },
//            WAVPAK = new string[] { "*.wv" },
//            MAXIS = new string[] { "*.xa" },
//            FASTTRACKER = new string[] { "*.xi" },
//            MPEG = new string[] { "*.mp3", "*.mp2" },
//        #endregion Audio Formats
//        #region Image Formats
//            JPEG = new string[] { "*.jpg", "*.jpeg", "*.jpe", "*.jfif", "*.jif" },
//            PNG = new string[] { "*.png" },
//            BMP = new string[] { "*.bmp", "*.dib" },
//            GIF = new string[] { "*.gif" },
//            TGA = new string[] { "*.tga", "*.icb", "*.vda", "*.vst" };
//        #endregion Image Formats

//        private static readonly List<FilterItem> AudioFileFamilies = new List<FilterItem>()
//        {
//            new FilterItem("Raw files", RAW),
//            new FilterItem("Amiga 8SVX", AMIGA),
//            new FilterItem("AIFF", AIFF),
//            new FilterItem("Ambisonic B", AMB),
//            new FilterItem("Sun Microsystems", SUN),
//            new FilterItem("Audio Visual Research", AVR),
//            new FilterItem("Red Book Compact Disk", RED_CD),
//            new FilterItem("Continuously Variable Slope Delta Modulation", CVSDM),
//            new FilterItem("Data", DATA),
//            new FilterItem("German Voicemail", DE_VM),
//            new FilterItem("Adaptive Multi Rate", AMR),
//            new FilterItem("Apple Core Audio", APPLE_CORE),
//            new FilterItem("Ensoniq PARIS", PARIS),
//            new FilterItem("FLAC", FLAC, true),
//            new FilterItem("Grandstream", GRANDSTREAM),
//            new FilterItem("GSM", GSM),
//            new FilterItem("HCOM", HCOM),
//            new FilterItem("HTK", HTK),
//            new FilterItem("IRCAM SDIF", IRCAM),
//            new FilterItem("IMA", IMA),
//            new FilterItem("LPC-10", LPC),
//            new FilterItem("MatLab", MATLAB),
//            new FilterItem("MS MacroSystem", MACROSYS),
//            new FilterItem("SPHERE", SPHERE),
//            new FilterItem("Ogg Vorbis", OGG, true),
//            new FilterItem("Opus", OPUS),
//            new FilterItem("Psion", PSION),
//            new FilterItem("Portable Voice", PORTABLE_VOICE),
//            new FilterItem("Sound Designer 2", SOUND_DESIGN),
//            new FilterItem("MIDI Sample", MIDI),
//            new FilterItem("Asterisk PBX", ASTERISK),
//            new FilterItem("Turtle Beach SampleVision", TURTLE_BEACH),
//            new FilterItem("Sounder", SOUNDER),
//            new FilterItem("SoundTool", SOUND_TOOL),
//            new FilterItem("SoX Native", SOX),
//            new FilterItem("Yamaha TX-16W", YAMAHA),
//            new FilterItem("Sound Blaster", SOUND_BLASTER),
//            new FilterItem("Dialogic / OKI", DIALOGIC),
//            new FilterItem("Sonic Foundry", SONIC_FOUNDRY),
//            new FilterItem("Waveform", WAV, true),
//            new FilterItem("WavPack", WAVPAK),
//            new FilterItem("Maxis", MAXIS),
//            new FilterItem("Fasttracker 2", FASTTRACKER),
//            new FilterItem("MPEG", MPEG, true)
//        };
//        private static readonly List<FilterItem> ImageFileFamilies = new List<FilterItem>()
//        {
//            new FilterItem("JPEG", JPEG, true),
//            new FilterItem("PNG", PNG, true),
//            new FilterItem("BMP", BMP),
//            new FilterItem("GIF", GIF),
//            new FilterItem("TGA", TGA)
//        };

//        private struct FilterItem
//        {
//            public string Name { get; private set; }
//            public string[] Extensions { get; private set; }
//            public bool Common { get; private set; }
//            public string FilterString { get; private set; }

//            public FilterItem(string name, string[] extensions, bool common = false)
//            {
//                Name = name;
//                Extensions = extensions;
//                Common = common;
//                FilterString = string.Format(CultureInfo.CurrentCulture, "{0} ({1})|{2}", Name, string.Format(CultureInfo.CurrentCulture, Extensions.Length > 10 ? "{0}..." : "{0}", string.Join(", ", Extensions.Take(10))), string.Join(';', Extensions));
//            }
//        }

//        private enum FilterType
//        {
//            Audio,
//            Image
//        }

//        public FileFilter(/*Control.SoxInterop soxInterop*/)
//        {
//            _soxInterop = soxInterop ?? throw new ArgumentNullException(nameof(soxInterop));
//            _soxInterop.PropertyChanged += (s, e) =>
//            {
//                if (e.PropertyName.Equals(nameof(Control.SoxInterop.HasSox), StringComparison.Ordinal))
//                    AudioFilter = GenerateFilter(FilterType.Audio);
//            };
//            AudioFileFamilies.Sort((l, r) => string.Compare(l.Name, r.Name, StringComparison.CurrentCultureIgnoreCase));
//            AudioFilter = GenerateFilter(FilterType.Audio);
//            ImageFilter = GenerateFilter(FilterType.Image);
//        }
//        private readonly Control.SoxInterop _soxInterop;

//        public string AudioFilter { get; private set; }
//        public string ImageFilter { get; private set; }

//        private string GenerateFilter(FilterType filterType)
//        {
//            List<FilterItem> workingList;

//            switch (filterType)
//            {
//                case FilterType.Audio:
//                    workingList = AudioFileFamilies.Select(filterItem => filterItem).ToList();
//                    if (!_soxInterop.HasSox)
//                        workingList.RemoveAll(filterItem => filterItem.Extensions.Equals(MPEG));
//                    break;
//                case FilterType.Image:
//                    workingList = ImageFileFamilies.Select(filterItem => filterItem).ToList();
//                    break;
//                default:
//                    throw new InvalidOperationException();
//            }

//            var individuals = string.Join('|', workingList.Select(filterItem => filterItem.FilterString));
//            var all = string.Join(';', workingList.Select(filterItem => filterItem.Extensions).SelectMany(extensions => extensions));
//            var allDescriptor = string.Join(" ,", workingList.Select(filterItem => filterItem.Extensions).SelectMany(extensions => extensions).Take(10));
//            var common = string.Join(';', workingList.Where(filterItem => filterItem.Common).Select(commonFilterItem => commonFilterItem.Extensions).SelectMany(extensions => extensions));
//            var commonDescriptor = string.Join(" ,", workingList.Where(filterItem => filterItem.Common).Select(commonFilterItem => commonFilterItem.Extensions).SelectMany(extensions => extensions).Take(10));

//            return string.Format(CultureInfo.CurrentCulture, "Common formats ({0})|{1}|All supported formats ({2}...)|{3}|All files|*.*|{4}", commonDescriptor, common, allDescriptor, all, individuals);
//        }
//    }
//}
