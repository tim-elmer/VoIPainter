using NAudio.Wave;
using System;
using System.ComponentModel;
using System.IO;
using VoIPainter.Model;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles formatting of ringtones
    /// </summary>
    public class RingtoneReformatController : INotifyPropertyChanged
    {
        private string _path;
        private readonly MainController _mainController;
        private readonly LogController _logController;
        private byte[] _audio;
        public event PropertyChangedEventHandler PropertyChanged;

        internal static readonly WaveFormat WAVE_FORMAT = WaveFormat.CreateMuLawFormat(8_000, 1);

        /// <summary>
        /// The path to the ringtone
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                Format();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
            }
        }

        /// <summary>
        /// The formatted ringtone as a memory stream
        /// </summary>
        public Stream Ringtone { get; private set; }

        public RingtoneReformatController(MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));

            _mainController.SettingsController.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SettingsController.LastModel):
                    case nameof(SettingsController.FadeOutTime):
                        Format();
                        break;                        
                }
            };
            _logController = _mainController.LogController;
        }


        /// <summary>
        /// Read and format the ringtone
        /// </summary>
        public void Format()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return;

            _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Info, Strings.StatusFormattingTone));
            try
            {
                if (!(Ringtone is null))
                    Ringtone = null;

                // Load file
                using var reader = new AudioFileReader(Path);

                // Conform to 8KHz 16b x1
                var conform = new WaveFormat(8_000, 16, 1);
                using var resampler = new MediaFoundationResampler(reader, conform);

                // Fade out
                var fader = new FadeOutToSampleProvider(resampler.ToSampleProvider());
                fader.FadeOutTo(Phone.RingLength(_mainController.SettingsController.LastModel), _mainController.SettingsController.FadeOutTime);
                var fadeBuffer = new float[Phone.RingLength(_mainController.SettingsController.LastModel)];
                fader.Read(fadeBuffer, 0, fadeBuffer.Length);

                // Convert from samples to PCM
                var streamBuffer = GetSamplesWaveData(fadeBuffer, fadeBuffer.Length);
                using var stream = new RawSourceWaveStream(new MemoryStream(streamBuffer), conform);

                // Encode to MuLaw
                using var encoder = new WaveFormatConversionStream(WAVE_FORMAT, stream);
                _audio = new byte[Phone.RingLength(_mainController.SettingsController.LastModel)];
                encoder.Read(_audio, 0, _audio.Length);

                // Apply to property
                Ringtone = new RawSourceWaveStream(new MemoryStream(_audio, false), WAVE_FORMAT);

                _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Info, Strings.StatusFormattingToneDone));
            }
            catch (Exception e)
            {
                _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Error, e.Message));
            }
        }

        // Adapted from https://stackoverflow.com/a/42151979
        private static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
        {
            if (samples is null)
                throw new ArgumentNullException(nameof(samples));

            var pcm = new byte[samplesCount * 2];
            int sampleIndex = 0,
                pcmIndex = 0;

            while (sampleIndex < samplesCount)
            {
                var outsample = (short)(samples[sampleIndex] * short.MaxValue);
                pcm[pcmIndex] = (byte)(outsample & 0xff);
                pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            return pcm;
        }
    }
}
