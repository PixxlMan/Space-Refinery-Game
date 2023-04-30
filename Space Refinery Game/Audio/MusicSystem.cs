using FixedPrecision;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game.Audio
{
	public class MusicSystem
	{
		public MusicSystem(AudioWorld audioWorld)
		{
			this.audioWorld = audioWorld;

			MainGame.GlobalSettings.RegisterToSetting<SliderSetting>("Music volume", (volumeSlider) => MusicVolume = volumeSlider.Value / 100);
		}

		private FixedDecimalLong8 musicVolume;

		private object SyncRoot = new();

		private AudioWorld audioWorld;

		private HashSet<MusicResource> musicSet = new();

		private Dictionary<MusicTag, HashSet<MusicResource>> musicByTag = new();

		private Dictionary<string, MusicResource> musicByName = new();

		private Queue<MusicResource> musicQueue = new();

		private HashSet<MusicTag> musicTags = new();

		/// <summary>
		/// Setting the value below zero or above one will result in the value being clamped to whichever is closest.
		/// </summary>
		public FixedDecimalLong8 MusicVolume
		{
			get => musicVolume;
			set => FixedDecimalLong8.Clamp(musicVolume = value, 0, 1);
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

		public void PlayNext()
		{
			lock (SyncRoot)
			{
				if (musicQueue.Count == 0)
				{
					return;
				}

				var music = musicQueue.Dequeue();
				music.Tracks[0].Play(musicVolume);
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
	}

	public enum MusicTag
	{
		MainMenu,
		Calm,
		Intense,
	}
}
