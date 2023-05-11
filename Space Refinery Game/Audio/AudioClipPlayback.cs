using NVorbis;
using SharpAudio.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vortice;

namespace Space_Refinery_Game.Audio
{
	public sealed class AudioClipPlayback
	{
		private readonly VorbisReader decoder;

		public AudioClipPlayback(string path)
		{
			Logging.Log($"Streaming audio file from path '{Path.GetFullPath(path)}'.");

			decoder = new VorbisReader(path);
		}

		/// <summary>
		/// Converts a buffer from a 32 bit floating point format to a 16 bit integer format.
		/// </summary>
		private static void ConvertBuffer(ReadOnlySpan<float> inBuffer, Span<byte> outBuffer) // From VorbisDecoder in SharpAudio
		{
			for (int i = 0; i < inBuffer.Length; i++)
			{
				int num = (int)(32767f * inBuffer[i]);
				if (num > 32767)
				{
					num = 32767;
				}
				else if (num < -32768)
				{
					num = -32768;
				}

				outBuffer[2 * i] = (byte)((uint)(short)num & 0xFFu);
				outBuffer[2 * i + 1] = (byte)((short)num >> 8);
			}
		}

		public void GetSamples(Span<byte> data)
		{
		}

		public bool IsFinished => decoder.IsEndOfStream;
	}
}
