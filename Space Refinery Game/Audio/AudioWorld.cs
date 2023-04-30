using SharpAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game.Audio
{
	public class AudioWorld
	{
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

			return audioWorld;
		}

		public MusicSystem MusicSystem { get; private set; }

		public AudioEngine AudioEngine { get; private set; }
	}
}
