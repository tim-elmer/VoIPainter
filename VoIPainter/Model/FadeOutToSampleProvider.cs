using NAudio.Wave;
using System;

namespace VoIPainter.Model
{
    /// <summary>
    /// Fades out a sample stream
    /// </summary>
    /// <remarks>Adapted from https://gist.github.com/markheath/8fb396a5fe4bf117f361</remarks>
    public class FadeOutToSampleProvider : ISampleProvider
    {
        private readonly object _lock = new object();
        private readonly ISampleProvider _source;
        private int _fadeOutSampleCount;
        private int _fadeOutDelaySamples;

        /// <summary>
        /// Creates a new FadeOutToSampleProvider
        /// </summary>
        /// <param name="source">The source stream with the audio to be faded in or out</param>
        public FadeOutToSampleProvider(ISampleProvider source) => 
            _source = source ?? throw new ArgumentNullException(nameof(source));

        /// <summary>
        /// Requests that a fade-out begins to the given target, from the given duration earlier
        /// </summary>
        /// <param name="fadeToSamples">The sample to end the fade at</param>
        /// <param name="fadeDuration">How long in seconds before <paramref name="fadeToSamples"/> to start the fade</param>
        public void FadeOutTo(int fadeToSamples, double fadeDuration)
        {
            lock (_lock)
            {
                _fadeOutSampleCount = (int)(fadeDuration * WaveFormat.SampleRate);
                _fadeOutDelaySamples = fadeToSamples - _fadeOutSampleCount;
            }
        }

        /// <inheritdoc cref="IWaveProvider.WaveFormat"/>
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <inheritdoc cref="IWaveProvider.Read(byte[], int, int)"/>
        public int Read(float[] buffer, int offset, int count)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            int sourceSamplesRead = 0;
            var stride = WaveFormat.Channels;


            for (var sample = offset; sample * stride + offset < count; sample += stride)
            {
                sourceSamplesRead += _source.Read(buffer, offset + sample, stride);

                lock (_lock)
                {
                    if (_fadeOutSampleCount <= 0 || sample * stride + offset < _fadeOutDelaySamples) ;
                    else if (sample * stride + offset > _fadeOutDelaySamples + _fadeOutSampleCount)
                        buffer[offset] = 0;
                    else
                        Fade(buffer, sample * stride + offset);
                }
            }
            return sourceSamplesRead;
        }

        /// <summary>
        /// Fade a sample based on its position in the fade
        /// </summary>
        /// <param name="buffer">Buffer of samples</param>
        /// <param name="offset">Sample to fade</param>
        private void Fade(float[] buffer, int offset)
        {
            if (offset > _fadeOutSampleCount + _fadeOutDelaySamples)
                buffer[offset] = 0;
            else
                for (int channel = 0; channel < _source.WaveFormat.Channels; channel++)
                    buffer[offset] *= 1f - (offset - _fadeOutDelaySamples) / (float)_fadeOutSampleCount;
        }
    }
}
