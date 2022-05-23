using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StegaDwt.UI
{
    public class WavFile
    {
        private const int ticksInSecond = 10000000;
        private TimeSpan duration;
        private int _channels;
        private int _sampleRate;
        private int _bytesPerSample;
        private List<float> _floatAudioBuffer = new List<float>();

        public string PathAudioFile { get; }
        public TimeSpan Duration { get { return duration; } }
        public float[] FloatAudioBuffer
        {
            get => _floatAudioBuffer.ToArray();
            set => _floatAudioBuffer = value.ToList();
        }
        public int SampleRate { get => _sampleRate; }
        public int Channels { get => _channels; }
        public int BytesPerSample { get => _bytesPerSample; }

        public WavFile(string path)
        {
            PathAudioFile = path;
            ReadWavFile(path);
        }

        private void ReadWavFile(string filename)
        {
            try
            {
                using var reader = new WaveFileReader(filename);

                float[] buffer;
                while ((buffer = reader.ReadNextSampleFrame()) != null)
                {
                    _floatAudioBuffer.AddRange(buffer);
                }

                _sampleRate = reader.WaveFormat.SampleRate;
                _channels = reader.WaveFormat.Channels;
                _bytesPerSample = reader.WaveFormat.BitsPerSample / 8;

                duration = DeterminateDurationTrack(_channels, _sampleRate);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("File reading error", ex);
            }
        }

        public void WriteWavFile(string fileName)
        {
            throw new NotImplementedException();
        }

        private TimeSpan DeterminateDurationTrack(int channels, int sampleRate)
        {
            long _duration = (long)((double)_floatAudioBuffer.Count / sampleRate / channels * ticksInSecond);
            return TimeSpan.FromTicks(_duration);
        }
    }
}
