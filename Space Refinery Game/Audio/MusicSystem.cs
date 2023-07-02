using FixedPrecision;
using ImGuiNET;
using Microsoft.VisualBasic;
using Space_Refinery_Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Multimedia;

namespace Space_Refinery_Game.Audio
{
	public class MusicSystem
	{
		private FixedDecimalLong8 musicVolume;

		private object SyncRoot = new();

		private AudioWorld audioWorld;

		private HashSet<MusicResource> musicSet = new();

		private Dictionary<MusicTag, HashSet<MusicResource>> musicByTag = new();

		private Dictionary<string, MusicResource> musicByName = new();

		private Queue<MusicResource> musicQueue = new();

		private HashSet<MusicTag> musicTags = new();

		private SequencialPlayback sequencialPlayback;

		private MusicResource currentMusic;

		private int loops;

		private int playedLoops;

		private MusicPart nextMusicPart;

		/// <summary>
		/// Setting the value below zero or above one will result in the value being clamped to whichever is closest.
		/// </summary>
		public FixedDecimalLong8 MusicVolume
		{
			get => musicVolume;
			set
			{
				musicVolume = FixedDecimalLong8.Clamp(value, 0, 1);

				VolumeChanged();
			}
		}

		public MusicSystem(GameData gameData, AudioWorld audioWorld /*gameData.AudioWorld can't be accessed because it hasn't been filled yet.*/)
		{
			this.audioWorld = audioWorld;

			sequencialPlayback = SequencialPlayback.Create(audioWorld.AudioEngine);

			sequencialPlayback.RequestNextClip += RequestNextClip;

			audioWorld.VolumeChanged += (_) => VolumeChanged();

			sequencialPlayback.Start();

			gameData.Settings.RegisterToSettingValue<SliderSettingValue>("Music Volume", (value) => MusicVolume = value.SliderValue / 100 );

			UI.DoDebugStatusUI += () =>
			{
				if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show music system debug status"))
				{
					ImGui.Separator();
					if (currentMusic is null)
					{
						ImGui.Text("Not playing anything.");
					}
					else
					{
						ImGui.Text($"Now playing: {currentMusic.Name}");
						ImGui.Text($"Next part: {nextMusicPart}");
						ImGui.Text($"Loops: {loops}");
						ImGui.Text($"Played loops: {playedLoops}");
					}
				}
			};
		}

		public void VolumeChanged()
		{
			lock (SyncRoot)
			{
				sequencialPlayback.VolumeChanged(MusicVolume * audioWorld.MasterVolume);
			}
		}

		private AudioClipPlayback RequestNextClip()
		{
			lock (SyncRoot)
			{
				if (currentMusic is null)
				{
					return NewMusic();
				}

				if (nextMusicPart == MusicPart.Loop)
				{
					if (playedLoops >= loops)
					{
						nextMusicPart = MusicPart.Outro;
					}
					playedLoops++;
					return currentMusic.Loops.SelectRandom().AudioResource.CreatePlayback();
				}

				if (nextMusicPart == MusicPart.Outro)
				{
					if (currentMusic.Outro is null)
					{
						return NewMusic();
					}
					else
					{
						var playback = currentMusic.Outro.AudioResource.CreatePlayback();
						currentMusic = null;
						playedLoops = 0;
						loops = Random.Shared.Next(1, 4);
						return playback;
					}
				}
			}

			throw new GlitchInTheMatrixException();

			AudioClipPlayback NewMusic()
			{
				if (musicQueue.Count == 0)
				{
					return null;
				}

				nextMusicPart = MusicPart.Loop;
				currentMusic = musicQueue.Dequeue();
				return currentMusic.Intro.AudioResource.CreatePlayback();
			}
		}

		public void RegisterMusic(MusicResource musicResource)
		{
			lock (SyncRoot)
			{
				musicSet.AddUnique(musicResource, $"This {nameof(MusicResource)} has already been registered.");

				musicByName.Add(musicResource.Name, musicResource);

				foreach (var tag in musicResource.Tags)
				{
					if (musicByTag.ContainsKey(tag))
					{
						musicByTag[tag].Add(musicResource);
					}
					else
					{
						musicByTag.Add(tag, new HashSet<MusicResource>() { musicResource });
					}
				}
			}
		}

		public void FillQueue()
		{
			lock (SyncRoot)
			{
				if (musicTags.Count == 0)
				{
					return;
				}
				else if (musicTags.Count == 1)
				{
					var tag = musicTags.First();
					if (musicByTag.ContainsKey(tag))
					{
						foreach (var musicResource in musicByTag[tag])
						{
							musicQueue.Enqueue(musicResource);
						}
					}
				}
				else
				{
					HashSet<MusicResource> matchingMusic = musicSet;

					foreach (MusicTag tag in musicTags)
					{
						if (musicByTag.ContainsKey(tag))
						{
							matchingMusic.IntersectWith(musicByTag[tag]);
						}
					}

					foreach (var musicResource in matchingMusic)
					{
						musicQueue.Enqueue(musicResource);
					}
				}
			}
		}

		public void SetTags(ISet<MusicTag> musicTags)
		{
			lock (SyncRoot)
			{
				this.musicTags = (HashSet<MusicTag>)musicTags;
			}
		}

		public void SetTags(MusicTag musicTag)
		{
			lock (SyncRoot)
			{
				musicTags = new() { musicTag };
			}
		}

		public void Clear()
		{
			// TODO: implement this once requirements are clearer
		}
	}

	public enum MusicTag
	{
		MainMenu,
		Calm,
		Intense,
	}
}
