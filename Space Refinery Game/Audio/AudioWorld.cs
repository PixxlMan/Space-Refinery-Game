using FixedPrecision;
using SharpAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game.Audio
{
	public class AudioWorld
	{
		private FixedDecimalLong8 masterVolume;

		private AudioWorld()
		{ }

		public static AudioWorld Create()
		{
			AudioWorld audioWorld = new();

			audioWorld.AudioEngine = AudioEngine.CreateOpenAL();

			if (audioWorld.AudioEngine is null)
			{
				throw new Exception("Could not create audio engine!");
			}

			audioWorld.MusicSystem = new(audioWorld);

			MainGame.GlobalSettings.RegisterToSetting<SliderSetting>("Master volume", (volumeSlider) => audioWorld.MasterVolume = volumeSlider.Value / 100);

			return audioWorld;
		}

		/// <summary>
		/// Setting the value below zero or above one will result in the value being clamped to whichever is closest.
		/// </summary>
		public FixedDecimalLong8 MasterVolume
		{
			get => masterVolume;
			set => FixedDecimalLong8.Clamp(masterVolume = value, 0, 1);
		}

		public MusicSystem MusicSystem { get; private set; }

		public AudioEngine AudioEngine { get; private set; }
	}
}
