using FixedPrecision;
using SharpAudio;

namespace Space_Refinery_Engine.Audio
{
	public class AudioWorld
	{
		private FixedDecimalLong8 masterVolume;

		/// <summary>
		/// Setting the value below zero or above one will result in the value being clamped to whichever is closest.
		/// </summary>
		public FixedDecimalLong8 MasterVolume
		{
			get => masterVolume;
			set
			{
				masterVolume = FixedDecimalLong8.Clamp(value, 0, 1);

				VolumeChanged.InvokeAndPurge(masterVolume);
			}
		}

		public WeakEvent<FixedDecimalLong8> VolumeChanged = new();

		public MusicSystem MusicSystem { get; private set; }

		public AudioEngine AudioEngine { get; private set; }

		private AudioWorld()
		{ }

		public static AudioWorld Create(GameData gameData)
		{
			AudioWorld audioWorld = new();

			audioWorld.AudioEngine = AudioEngine.CreateOpenAL();

			if (audioWorld.AudioEngine is null)
			{
				throw new Exception("Could not create audio engine!");
			}

			audioWorld.MusicSystem = new(gameData, audioWorld);

			gameData.Settings.RegisterToSettingValue<SliderSettingValue>("Main Volume", (value) => audioWorld.MasterVolume = value.SliderValue / 100);

			return audioWorld;
		}

		public void Reset()
		{
			MusicSystem.Clear();
		}
	}
}
