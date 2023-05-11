using BepuPhysics.Constraints;
using FixedPrecision;
using SharpAudio;
using SharpAudio.Codec;
using System.Runtime.CompilerServices;

namespace Space_Refinery_Game.Audio
{
	///	<summary>
	///	Supports sequencial playback of audio via callback.
	///	</summary>
	/// <remarks>
	/// Thread safe.
	/// </remarks>
	public sealed class SequencialPlayback
	{
		private SequencialPlayback(AudioEngine audioEngine)
		{
			AudioEngine = audioEngine;
		}

		public static SequencialPlayback Create(AudioEngine audioEngine)
		{
			SequencialPlayback sequencialPlaybackSystem = new(audioEngine);

			return sequencialPlaybackSystem;
		}

		public AudioClipPlayback? PlayingClip { get { lock (SyncRoot) return playingClip; } }
		
		public event Func<AudioClipPlayback?> RequestNextClip;

		private object SyncRoot = new();

		public bool Running { get { lock (SyncRoot) return running; } }

		public AudioEngine AudioEngine { get; }

		public AudioSource Source { get; private set; }

		private bool shouldStop = false;
		private AudioClipPlayback? playingClip = null;
		private bool running = false;

		public void Start()
		{
			lock (SyncRoot)
			{
				if (Running)
				{
					return;
				}

				running = true;
				Thread thread = new(Run) { Name = "Sequencial Playback Thread" };
				thread.Start();
			}
		}

		public void Stop()
		{
			lock (SyncRoot)
			{
				if (!Running)
				{
					return;
				}

				shouldStop = true;
			}
		}

		private void InitializeSource()
		{

		}

		private void DisposeSource()
		{
			Source?.Stop();
			Source?.Dispose();
		}

		private void Run()
		{
			lock (SyncRoot)
			{
				InitializeSource();
			}

			while (true)
			{
				lock (SyncRoot)
				{
					if (shouldStop)
					{
						running = false;
						DisposeSource();
						return;
					}

					if (Source is null || !Source.IsPlaying())
					{
						Thread.Sleep(TimeSpan.FromSeconds(0.5));
						InitializeSource();
						continue;
					}

					if (Source.BuffersQueued >= 3)
					{
						continue;
					}


				}
			}
			
			void FillSilence()
			{

			}

			void FillSamples()
			{

			}
		}

		public void VolumeChanged(FixedDecimalLong8 volume)
		{
			Source.Volume = (float)FixedDecimalLong8.Clamp(volume, 0, 1);
		}
	}
}
