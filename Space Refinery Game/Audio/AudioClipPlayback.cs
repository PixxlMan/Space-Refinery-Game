using NVorbis;
using SharpAudio;
using SharpAudio.Codec;
using Space_Refinery_Utilities;
using System;
using Vortice;
using Vortice.DXGI;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Space_Refinery_Game.Audio
{
	/// <remarks>
	/// Not thread safe.
	/// </remarks>
	public sealed class AudioClipPlayback
	{
		// From SoundStream
		private byte[] _data;
		//private readonly Decoder _decoder;
		private readonly VorbisReader _reader;
		private static readonly TimeSpan SampleQuantum = TimeSpan.FromSeconds(0.05);
		private int sampleQuantumToNumSamples => (int)(SampleQuantum.TotalSeconds * _audioFormat.SampleRate * _audioFormat.Channels);

		// From VorbisDecoder
		private float[] _readBuf;

		// From Decoder
		protected AudioFormat _audioFormat;
		protected int _numSamples = 0;
		protected int _readSize;

		public AudioClipPlayback(string path)
		{
			Logging.Log($"Streaming audio file from path '{Path.GetFullPath(path)}'.");
			_reader = new VorbisReader(path);

			_audioFormat.Channels = _reader.Channels;
			_audioFormat.BitsPerSample = 16;
			_audioFormat.SampleRate = _reader.SampleRate;

			_numSamples = (int)_reader.TotalSamples;
		}

		public bool SubmitSamples(SequencialPlayback sequencialPlayback)
		{
			var res = GetSamples(sampleQuantumToNumSamples, ref _data);

			if (res == 0)
			{
				return false;
			}

			if (res == -1)
			{
				return false;
			}

			sequencialPlayback.Send(_data);

			return true;
		}

		private static void CastBuffer(float[] inBuffer, byte[] outBuffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				var temp = (int)(short.MaxValue * inBuffer[i]);

				if (temp > short.MaxValue)
				{
					temp = short.MaxValue;
				}
				else if (temp < short.MinValue)
				{
					temp = short.MinValue;
				}

				outBuffer[2 * i] = (byte)(((short)temp) & 0xFF);
				outBuffer[2 * i + 1] = (byte)(((short)temp) >> 8);
			}
		}

		private long GetSamples(int samples, ref byte[] data)
		{
			int bytes = _audioFormat.BytesPerSample * samples;
			Array.Resize(ref data, bytes);

			Array.Resize(ref _readBuf, samples);
			_reader.ReadSamples(_readBuf, 0, samples);

			CastBuffer(_readBuf, data, samples);

			return samples;
		}

		public void Dispose()
		{
			_reader.Dispose();
		}

		public bool IsFinished => _reader.IsEndOfStream;
	}
}
