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
			Engine = audioEngine;

			_format = new AudioFormat { SampleRate = 44_100, Channels = 2, BitsPerSample = 16 };

			var silenceDataCount = (int)(_format.Channels * _format.SampleRate * sizeof(ushort) * SampleQuantum.TotalSeconds);

			_silenceData = new byte[silenceDataCount];

			_chain = new BufferChain(Engine);
			_circBuffer = new CircularBuffer(_silenceData.Length);
			_tempBuf = new byte[_silenceData.Length];
			_submixer = null;
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

		public AudioEngine Engine { get; }

		public AudioSource Source { get; private set; }

		private bool shouldStop = false;
		private AudioClipPlayback? playingClip = null;
		private bool running = false;
		private Thread playbackThread;

		// Playback technical data
		private static readonly TimeSpan SampleQuantum = TimeSpan.FromSeconds(0.05);
		private readonly BufferChain _chain;
		private readonly CircularBuffer _circBuffer;
		private readonly AudioFormat _format;
		private readonly byte[] _silenceData;
		private readonly byte[] _tempBuf;
		private readonly Submixer _submixer;

		public void Start()
		{
			lock (SyncRoot)
			{
				if (Running)
				{
					return;
				}

				running = true;

				playbackThread = new(Run) { Name = "Sequencial Playback Thread" };
				playbackThread.Start();
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
				playbackThread.Join();
			}
		}

		private void InitializeSource()
		{
			Source?.Dispose();
			Source = Engine.CreateSource(_submixer);
			_chain.QueueData(Source, _silenceData, _format);
			Source.Play();
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

					if (playingClip is null || playingClip.IsFinished)
					{
						playingClip = RequestNextClip?.Invoke();
					}

					if (playingClip is not null)
					{
						if (playingClip.SubmitSamples(this))
						{
							// Data was submitted.
						}
						else
						{
							// Something (end of stream or an error) prevented this clip from submitting data.
						}
					}

					var cL = _circBuffer.Length;
					var tL = _tempBuf.Length;

					if (cL >= tL)
					{
						_circBuffer.Read(_tempBuf, 0, _tempBuf.Length);
						_chain.QueueData(Source, _tempBuf, _format);
					}
					else if ((cL < tL) & (cL > 0))
					{
						var remainingSamples = new byte[cL];
						_circBuffer.Read(remainingSamples, 0, remainingSamples.Length);

						Buffer.BlockCopy(remainingSamples, 0, _tempBuf, 0, remainingSamples.Length);
						_chain.QueueData(Source, _tempBuf, _format);
					}
					else
					{
						_chain.QueueData(Source, _silenceData, _format);
					}
				}
			}
		}

		public void Send(byte[] data)
		{
			_circBuffer.Write(data, 0, data.Length);
		}

		internal void ClearBuffers()
		{
			_circBuffer.Clear();
		}

		public void VolumeChanged(FixedDecimalLong8 volume)
		{
			Source.Volume = (float)FixedDecimalLong8.Clamp(volume, 0, 1);
		}
	}
}
