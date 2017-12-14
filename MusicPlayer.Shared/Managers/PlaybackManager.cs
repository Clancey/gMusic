using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Models;
using MusicPlayer.Data;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Timers;
using MusicPlayer.Api;
using MusicPlayer.Models.Scrobbling;
using MusicPlayer.ViewModels;
using SimpleDatabase;
using MusicPlayer.Playback;

namespace MusicPlayer.Managers
{
	internal class PlaybackManager : ManagerBase<PlaybackManager>
	{
		public NativeAudioPlayer NativePlayer;
		readonly Timer killTimer;

		public PlaybackManager()
		{
			killTimer = new Timer(20*1000);
			killTimer.Elapsed += (sender, args) => StopTimers();
			NativePlayer = new NativeAudioPlayer();
			//LocalWebServer.Shared.Start(10);
			NativePlayer.SubscribeToProperty(nameof (NativePlayer.State), () =>
			{
				NotificationManager.Shared.ProcPlaybackStateChanged(NativePlayer.State);
				if (NativePlayer.State == PlaybackState.Playing || NativePlayer.State == PlaybackState.Buffering)
				{
					StartTimers();
					killTimer.Stop();
				}
				else
				{
					killTimer.Start();
				}
			});
			NativePlayer.SubscribeToProperty("CurrentSong", () =>
			{
				var songID = NativePlayer?.CurrentSong?.Id ?? "";
				if (songID != Settings.CurrentSong)
					Settings.CurrentPlaybackPercent = 0;
				Settings.CurrentSong = NativePlayer?.CurrentSong?.Id ?? "";
				NotificationManager.Shared.ProcCurrentSongChanged(NativePlayer.CurrentSong);
			});
			NotificationManager.Shared.OfflineChanged += Shared_OfflineChanged;
			NotificationManager.Shared.FailedDownload += NotificationManager_Shared_FailedDownload;
        }

		void NotificationManager_Shared_FailedDownload (object sender, SimpleTables.EventArgs<string> e)
		{
			if(e.Data == Settings.CurrentSong)
				NextTrack ();
		}

		private void Shared_OfflineChanged(object sender, EventArgs e)
		{
			SetUpOffline(Settings.CurrentSong);
		}

		public void Init()
		{
			#if __MACOS__
			KeyboardControlHandler.Init();
			#endif
			#if !__ANDROID__
			NativeTrackHandler.Shared.Init();
			RemoteControlHandler.Init();
			#endif
			LoadPlaylist();
		}

		public List<int> CurrentOrder = new List<int>();
		public int CurrentSongIndex = 0;
		static int currentPlaylistSongCount;

		public int CurrentPlaylistSongCount
		{
			get { return CurrentOrder?.Count ?? 0; }
			set { currentPlaylistSongCount = value; }
		}

		public void PlayPause()
		{
			switch (NativePlayer.State)
			{
				case PlaybackState.Buffering:
				case PlaybackState.Playing:
					Pause();
					StopTimers();
					return;
				default:
					Play ();
					return;
			}
		}

		public void Pause()
		{
			NativePlayer.Pause();
			LogManager.Shared.PressPause ();
		}

		void StartTimers()
		{
			killTimer.Start();
			ProgressTimer.Start();
			VisualizerTimer.Start();
			App.Playing();
		}

		void StopTimers()
		{
			killTimer.Stop();
			ProgressTimer.Stop();
			VisualizerTimer.Stop();
			App.StoppedPlaying();
		}


		Timer progressTimer;

		public Timer ProgressTimer
		{
			get
			{
				if (progressTimer != null) return progressTimer;

				progressTimer = new Timer(500);
				progressTimer.Elapsed += (sender, args) =>
				{
					NotificationManager.Shared.ProcCurrentTrackPositionChanged(new TrackPosition
					{
						CurrentTime = NativePlayer.CurrentTime,
						Duration = NativePlayer.Duration,
					});
					var p = NativePlayer.Progress;
					if (NativePlayer.State == PlaybackState.Playing && p > 0)
						Settings.CurrentPlaybackPercent = p;
				};
				return progressTimer;
			}
			set { progressTimer = value; }
		}

		Timer visualizerTimer;

		public Timer VisualizerTimer
		{
			get
			{
				if (visualizerTimer != null) return visualizerTimer;
				visualizerTimer = new Timer(100);
				visualizerTimer.Elapsed +=
					(sender, args) => NotificationManager.Shared.ProcUpdateVisualizer(NativePlayer?.AudioLevels ?? new float[] {0, 0});
				return visualizerTimer;
			}
			set { visualizerTimer = value; }
		}

		public void Play()
		{
			LogManager.Shared.PressedPlay ();
			if(CurrentPlaylistSongCount > 0){
				NativePlayer.Play();
				return;
			}
			Play (null, Database.Main.GetGroupInfo<Song> ());
		}

		public async Task PlayNow(Song song, bool playVideo = false)
		{
			if(!CurrentOrder.Any())
			{
				await Play(song,Database.Main.GetGroupInfo<Song> (), playVideo: playVideo);
				return;
			}
            if (song.Id == Settings.CurrentSong)
			{
				await NativePlayer.PlaySong(song, playVideo);
				return;
			}


			PlayNext(song);
			await NextTrack(playVideo);
			
		}
		public async Task Play(Song song, GroupInfo groupInfo, bool includeOneStar = false, bool playVideo = false)
		{
			SendEndNotification(ScrobbleManager.PlaybackEndedReason.Skipped);
			Settings.CurrentPlaybackContext = new PlaybackContext
			{
				IsContinuous = false,
				Type = PlaybackContext.PlaybackType.Song,
			};

			Pause();
			var info = groupInfo.Clone();
			await Task.WhenAll(
				NativePlayer.PlaySong(song),
				Task.Run(async () =>
				{
					if (!includeOneStar)
						info.Filter += (string.IsNullOrEmpty(info.Filter) ? "" : " and ") + "Rating <> 1";
					string query = $"select Id from Song {info.FilterString(true)} {info.OrderByString(true)} {info.LimitString()}";
					var queryInfo = info.ConvertSqlFromNamed(query);
					await SetupCurrentPlaylist(queryInfo.Item1, song?.Id ?? "", queryInfo.Item2);
				}));
			if (song == null)
			{
				song = GetSong(CurrentSongIndex);
				await NativePlayer.PlaySong(song);
			}
			await PrepareNextTrack();
		}

		public async Task Play(OnlinePlaylistEntry entry, OnlinePlaylist playlist)
		{
			LogManager.Shared.LogPlayback(entry);
			using (new Spinner("Loading Playlist"))
			{
				await MusicManager.Shared.AddTemp(playlist);
			}
			var groupInfo = PlaylistSongViewModel.CreateGroupInfo(playlist);
			await PlayTempPlaylist(entry, groupInfo, playlist.Id);
		}

		public async Task PlayTempPlaylist(PlaylistSong playlistSong, GroupInfo groupInfo, string playlistId = "")
		{
			SendEndNotification(ScrobbleManager.PlaybackEndedReason.Skipped);
			Settings.CurrentPlaybackContext = new PlaybackContext
			{
				IsContinuous = false,
				Type = PlaybackContext.PlaybackType.Playlist,
				ParentId = playlistSong?.PlaylistId ?? playlistId,
			};
			Pause();
			var song = playlistSong == null ? null : Database.Main.GetObject<Song, TempSong>(playlistSong.SongId);
			var info = groupInfo.Clone();
			await Task.WhenAll(
				NativePlayer.PlaySong(song),
				Task.Run(async () =>
				{
					string query =
						$"select SongId as Id from TempPlaylistSong {info.FilterString(true)} {info.OrderByString(true)} {info.LimitString()}";
					var queryInfo = info.ConvertSqlFromNamed(query);
					await SetupCurrentPlaylist(queryInfo.Item1, song?.Id ?? "", queryInfo.Item2);
				}));
			if (song == null)
			{
				song = GetSong(CurrentSongIndex);
				await NativePlayer.PlaySong(song);
			}
			await PrepareNextTrack();
		}

		public async Task PlayPlaylist(Playlist playlist)
		{
			LogManager.Shared.LogPlayback(playlist);
			var groupInfo = PlaylistSongViewModel.CreateGroupInfo(playlist);
			await PlayPlaylist(null, groupInfo, playlist.Id);
		}

		public async Task PlayPlaylist(PlaylistSong playlistSong, GroupInfo groupInfo, string playlistId = "")
		{

			LogManager.Shared.LogPlayback (playlistSong);
			SendEndNotification(ScrobbleManager.PlaybackEndedReason.Skipped);
			Settings.CurrentPlaybackContext = new PlaybackContext
			{
				IsContinuous = false,
				Type = PlaybackContext.PlaybackType.Playlist,
				ParentId = playlistSong?.PlaylistId ?? playlistId,
			};
			Pause();
			var song = playlistSong == null ? null : Database.Main.GetObject<Song, TempSong>(playlistSong.SongId);
			var info = groupInfo.Clone();
			await Task.WhenAll(
				NativePlayer.PlaySong(song),
				Task.Run(async () =>
				{
					string query =
						$"select SongId as Id from PlaylistSong {info.FilterString(true)} {info.OrderByString(true)} {info.LimitString()}";
					var queryInfo = info.ConvertSqlFromNamed(query);
					await SetupCurrentPlaylist(queryInfo.Item1, song?.Id ?? "", queryInfo.Item2);
				}));
			if (song == null)
			{
				song = GetSong(CurrentSongIndex);
				await NativePlayer.PlaySong(song);
			}
			await PrepareNextTrack();
		}
		public async Task PlayAutoPlaylist(AutoPlaylist playlist,Song playlistSong, GroupInfo groupInfo = null)
		{
			if (groupInfo == null)
				groupInfo = AutoPlaylistSongViewModel.CreateGroupInfo(playlist, Settings.ShowOfflineOnly);
			LogManager.Shared.LogPlayback(playlist);
			SendEndNotification(ScrobbleManager.PlaybackEndedReason.Skipped);
			Settings.CurrentPlaybackContext = new PlaybackContext
			{
				IsContinuous = false,
				Type = PlaybackContext.PlaybackType.Playlist,
				ParentId = playlist.Id,
			};
			Pause();
			var song = playlistSong;
			var info = groupInfo.Clone();
			await Task.WhenAll(
				NativePlayer.PlaySong(song),
				Task.Run(async () =>
				{
					string query =
						$"select Id from Song {info.FilterString(true)} {info.OrderByString(true)} {info.LimitString()}";
					var queryInfo = info.ConvertSqlFromNamed(query);
					await SetupCurrentPlaylist(queryInfo.Item1, song?.Id ?? "", queryInfo.Item2);
				}));
			if (song == null)
			{
				song = GetSong(CurrentSongIndex);
				await NativePlayer.PlaySong(song);
			}
			await PrepareNextTrack();
		}

		public async Task Play(RadioStation station)
		{
			if(!await App.CheckForOffline())
				return;
			LogManager.Shared.LogPlay(station);
			var online = station as OnlineRadioStation;
			if (online != null)
			{
				await MusicManager.Shared.AddTemp(online);
			}
			SendEndNotification(ScrobbleManager.PlaybackEndedReason.Skipped);
			var context  = new PlaybackContext
			{
				IsContinuous = true,
				Type = PlaybackContext.PlaybackType.Radio,
				ParentId = station.Id
			};
			
			if(context.Equals(Settings.CurrentPlaybackContext) && CurrentPlaylistSongCount > 0) {
				Play();
				return;
			}

			using (new Spinner("Starting station"))
			{
				Settings.CurrentPlaybackContext = context;
				var success = await MusicManager.Shared.LoadRadioStationTracks(station);
				if (!success)
				{
					App.ShowAlert("Error", "Please try again later");
					return;
				}
			}

			if (Settings.CurrentPlaybackContext?.ParentId != station.Id)
				return;

			await Task.Run(async () =>
			{
				string query = $"select SongId as Id from RadioStationSong where StationId = ? order by SOrder";
				await SetupCurrentPlaylist(query, "", new[] { station.Id });
			});
			var song = GetSong(CurrentSongIndex);
			await NativePlayer.PlaySong(song);

			await PrepareNextTrack();
		}

		public async void Play(Song song, Album album, List<Song> songs)
		{
			LogManager.Shared.LogPlay (album);
			Settings.CurrentPlaybackContext = new PlaybackContext
			{
				IsContinuous = false,
				Type = PlaybackContext.PlaybackType.Album,
				ParentId = album.Id
			};
			await PlaySongs(songs,song);
		}

		public void PlaySong(string id)
		{
			var song = Database.Main.GetObject<Song>(id);
			Play(song);
		}
		public async void Play(MediaItemBase item)
		{
			SendEndNotification(ScrobbleManager.PlaybackEndedReason.Skipped);
			Settings.CurrentSong = "";
			var artist = item as Artist;
			if (artist != null)
			{
				var songs = await MusicManager.Shared.GetSongs(artist);
				Settings.CurrentPlaybackContext = new PlaybackContext
				{
					IsContinuous = false,
					Type = PlaybackContext.PlaybackType.Song,
				};
				await PlaySongs(songs);
				return;
			}

			var album = item as Album;
			if (album != null)
			{
				Settings.CurrentPlaybackContext = new PlaybackContext
				{
					IsContinuous = false,
					Type = PlaybackContext.PlaybackType.Album,
					ParentId = item.Id
				};
				var songs = await MusicManager.Shared.GetSongs(album);
				await PlaySongs(songs);
				return;
			}
			if (item is AutoPlaylist ap)
			{
				await PlayAutoPlaylist(ap, null);
				return;
			}
			var playlist = item as Playlist;
			if (playlist != null)
			{
				await PlayPlaylist(playlist);
				return;
			}

			var station = item as RadioStation;
			if (station != null)
			{
				await Play(station);
				return;
			}

			var genre = item as Genre;
			if (genre != null)
			{
				Settings.CurrentPlaybackContext = new PlaybackContext
				{
					IsContinuous = false,
					Type = PlaybackContext.PlaybackType.Genre,
					ParentId = item.Id
				};
				var songs = await MusicManager.Shared.GetSongs(genre);
				await PlaySongs(songs);
				return;
			}

			var song = item as Song;
			if (song != null)
			{
				await PlayNow(song);
				return;
			}
			App.ShowNotImplmented();
		}

		async Task PlaySongs(List<Song> songs, Song song = null)
		{
			Pause();
			await Task.WhenAll(
				Task.Run(async () => { await SetupCurrentPlaylist(songs, ""); }));
			if(song == null)
				song = SongAtIndex(CurrentSongIndex);
			await NativePlayer.PlaySong(song);

			await PrepareNextTrack();
		}

		void SendEndNotification(ScrobbleManager.PlaybackEndedReason reason)
		{
			if (string.IsNullOrWhiteSpace(Settings.CurrentSong))
				return;
			var song = Database.Main.GetObject<Song, TempSong>(Settings.CurrentSong);
			ScrobbleManager.Shared.PlaybackEnded(new PlaybackEndedEvent(song)
			{
				TrackId = Settings.CurrentTrackId,
				Context = Settings.CurrentPlaybackContext,
				Position = NativePlayer.CurrentTime,
				Reason = reason,
			});
		}

		public async Task PrepareNextTrack()
		{
			var nextIndex = NextIndex();
			if (nextIndex == -1)
				return;

			Console.WriteLine(Settings.CurrentPlaybackContext);
			if (Settings.CurrentPlaybackContext?.IsContinuous == true && (CurrentPlaylistSongCount - nextIndex) < 5)
			{
				Console.WriteLine("Getting more tracks!");
				await LoadMoreTracks();
			}

			if (CurrentOrder.Count <= nextIndex)
				return;
			var song = GetSong(nextIndex);
			if (song == null)
				return;

			var playbackData = await MusicManager.Shared.GetPlaybackData(song);
			NativePlayer.QueueTrack (playbackData.CurrentTrack);
			if (playbackData?.IsLocal == true)
				return;
			await DownloadManager.Shared.QueueTrack(playbackData.CurrentTrack.Id);
		}

		public Song GetSong(int index)
		{
			if (index >= CurrentOrder.Count)
				return null;
			var song = SongAtIndex(CurrentOrder[index]);
			return song;
		}

		Song SongAtIndex(int index)
		{
			try
			{
				Console.WriteLine(CurrentPlaylistSongCount);
				string query = $"select * from SongsOrdered where rowid = {index + 1}";

				var temp = Database.Main.Query<SongsOrdered>(query).First();

				return Database.Main.GetObject<Song, TempSong>(temp.Id);
			}
			catch (Exception ex)
			{
				//LogManager.Shared.Report(ex);
				return null;
			}
		}
		public async Task NextTrackWithoutPause(bool notification = false,bool playVideo = false)
		{
			var nextIndex = NextIndex();
			if(notification){
				SendEndNotification(nextIndex == CurrentSongIndex
					? ScrobbleManager.PlaybackEndedReason.StartedOver
					: ScrobbleManager.PlaybackEndedReason.Skipped);
			}
			if (nextIndex == -1)
			{
				NativePlayer.Seek(0);
				Pause();
				return;
			}
			if (CurrentSongIndex == nextIndex && Settings.RepeatMode != RepeatMode.RepeatOne)
				return;
			CurrentSongIndex = nextIndex;
			var song = GetSong(CurrentSongIndex);
			if (song == null)
				return;
			await NativePlayer.PlaySong(song, playVideo);
			await PrepareNextTrack();
		}
		public async Task NextTrack(bool playVideo = false)
		{
			LogManager.Shared.PressNext();
			Pause();
			await NextTrackWithoutPause(true,playVideo);
		}

		DateTime lastPreviousPressed = DateTime.Now;

		public void Previous()
		{
			if (string.IsNullOrWhiteSpace(Settings.CurrentSong))
				return;
			var diff = (DateTime.Now - lastPreviousPressed).TotalSeconds;
			Console.WriteLine("previous diff: " + diff);
			if (diff < 5 || NativePlayer.CurrentTime < 5)
				PlayPrevious();
			else
			{
				SendEndNotification(ScrobbleManager.PlaybackEndedReason.StartedOver);
				Seek(0);
				lastPreviousPressed = DateTime.Now;
			}
			LogManager.Shared.PressBack();
		}

		public void PlayPrevious()
		{
			SendEndNotification(ScrobbleManager.PlaybackEndedReason.Reverse);
			if (CurrentSongIndex <= 0)
			{
				if (Settings.RepeatMode == RepeatMode.RepeatAll)
					CurrentSongIndex = CurrentPlaylistSongCount - 1;
				else
					CurrentSongIndex = 0;
			}
			else
			{
				CurrentSongIndex--;
			}
			PlaySongAtIndex(CurrentSongIndex);
		}

		public void Seek(float percent)
		{
			if (string.IsNullOrEmpty (Settings.CurrentSong))
				return;
			Settings.CurrentPlaybackPercent = percent;
			NativePlayer.Seek(percent);
		}

		public int NextIndex()
		{
			if (CurrentSongIndex >= CurrentPlaylistSongCount - 1)
			{
				if (Settings.RepeatMode != RepeatMode.RepeatAll)
					return -1;
				if (Settings.ShuffleSongs)
					ShuffleCurrentPlaylist();
				return 0;
			}
			if (Settings.RepeatMode == RepeatMode.RepeatOne ||
				(CurrentPlaylistSongCount <= 1 && Settings.RepeatMode == RepeatMode.RepeatAll))
				return CurrentSongIndex;
			return CurrentSongIndex + 1;
		}


		Task loadMoreTracksTask;

		public async Task LoadMoreTracks()
		{
			if (loadMoreTracksTask?.IsCompleted == false)
			{
				await loadMoreTracksTask;
				return;
			}
			loadMoreTracksTask = Task.Run(async () =>
			{
				try
				{
					var station = Settings.CurrentPlaybackContext.ParentId == "IFL"
						? new RadioStation("I'm Feeling Lucky")
						{
							Id = "IFL",
						}
						: Database.Main.GetObject<RadioStation>(Settings.CurrentPlaybackContext.ParentId);
					if (station == null)
						return;

					var success = await MusicManager.Shared.LoadMoreRadioStationTracks(station);
					if (!success)
					{
						App.ShowAlert("Error", "Please try again later");
						return;
					}
					if (Settings.CurrentPlaybackContext?.ParentId != station.Id)
						return;

					string query = $"select SongId as Id from RadioStationSong where StationId = ? order by SOrder";
					await SetupCurrentPlaylist(query, NativePlayer?.CurrentSong?.Id ?? "", new object[] { station.Id });
				}
				catch (Exception ex)
				{
					LogManager.Shared.Report(ex);
				}
			});
			await loadMoreTracksTask;
		}

		public void ToggleRandom()
		{
			try
			{
				Settings.ShuffleSongs = !Settings.ShuffleSongs;
				if (Settings.CurrentPlaybackContext?.IsContinuous == true)
					return;
				if (CurrentPlaylistSongCount == 0)
					return;
				var curIndex = CurrentOrder[CurrentSongIndex];
				SetupCurrentOrder(Settings.CurrentSong);
				if (CurrentOrder.Count > 0)
				{
					if (Settings.ShuffleSongs)
					{
						ShuffleCurrentPlaylist();

						CurrentOrder.Remove(CurrentSongIndex);
						CurrentOrder.Insert(0, CurrentSongIndex);
						CurrentSongIndex = 0;
					}
					else
					{
						CurrentSongIndex = CurrentOrder.IndexOf(curIndex);
					}
				}
				NotificationManager.Shared.ProcCurrentPlaylistChanged();
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		public void ToggleRepeat()
		{
			var nextInt = (int) Settings.RepeatMode + 1;
			if (nextInt > 2)
				nextInt = 0;
			Settings.RepeatMode = (RepeatMode) nextInt;
		}


		public class SongsOrdered
		{
			public string Id { get; set; }
		}

		async Task<bool> SetupCurrentPlaylist(string query, string currentId, object[] parameters)
		{
			await ClearPlayist();
			Database.Main.Execute($"create table SongsOrdered as {query}", parameters);
			await PrepareCurrentPlaylist(currentId);

			NotificationManager.Shared.ProcCurrentPlaylistChanged();
			return true;
		}

		async Task<bool> SetupCurrentPlaylist(IEnumerable<Song> songs, string currentId)
		{
			await ClearPlayist();
			Database.Main.CreateTable<SongsOrdered>();
			Database.Main.InsertAll(songs.Select(x => new SongsOrdered() {Id = x.Id}));
			await PrepareCurrentPlaylist(currentId);

			NotificationManager.Shared.ProcCurrentPlaylistChanged();
			return true;
		}

		async Task ClearPlayist()
		{
			CurrentOrder.Clear();
			CurrentPlaylistSongCount = 0;
			CurrentSongIndex = 0;
			bool didClear = false;
			while (!didClear)
			{
				try
				{
					Database.Main.Execute("drop table if exists SongsOrdered");
					didClear = true;
				}
				catch
				{
					await Task.Delay(1000);
				}
			}

		}

		async Task<bool> PrepareCurrentPlaylist(string currentId)
		{
			CurrentSongIndex = string.IsNullOrEmpty(currentId)
				? 0
				: Database.Main.ExecuteScalar<int>("select rowid -1 from SongsOrdered where Id = ?", currentId);
			CurrentPlaylistSongCount = Database.Main.ExecuteScalar<int>("select count(*) from SongsOrdered");
			SetupCurrentOrder(currentId);
			if (CurrentOrder.Count == 0)
				return true;
			if (Settings.ShuffleSongs)
			{
				ShuffleCurrentPlaylist();
				if (!string.IsNullOrEmpty(currentId) && Settings.CurrentPlaybackContext?.IsContinuous == false)
				{
					CurrentOrder.Remove(CurrentSongIndex);
					CurrentOrder.Insert(0, CurrentSongIndex);
					CurrentSongIndex = 0;
				}
			}
			else
				SavePlaylist();
			return true;
		}

		void SetupCurrentOrder(string songId)
		{
			CurrentOrder.Clear();
			var count = currentPlaylistSongCount;
			//This is used for performance testing
			//count = 20000;
			CurrentOrder = Enumerable.Range(0, count).ToList();

			if (Settings.ShowOfflineOnly)
			{
				SetUpOffline(songId);
				return;
			}
		}

		class ReturnValue<T>
		{
			public T Value { get; set; }
		}

		async void SetUpOffline(string songId)
		{
			if (!string.IsNullOrEmpty(songId))
				CurrentSongIndex = Database.Main.ExecuteScalar<int>("select rowid -1 from SongsOrdered where Id = ?", songId);

			try
			{
				var query = "select so.rowid -1 as Value from SongsOrdered so" +
							(Settings.ShowOfflineOnly
								? " inner join Song s on so.Id = s.Id where s.OfflineCount > 0"
								: "");
				CurrentOrder = Database.Main.Query<ReturnValue<int>>(query).Select(x => x.Value).ToList();
				if (Settings.ShuffleSongs)
				{
					ShuffleCurrentPlaylist();
					CurrentOrder.Remove(CurrentSongIndex);
					CurrentOrder.Insert(0, CurrentSongIndex);
				}
				CurrentSongIndex = Math.Max(0, CurrentOrder.IndexOf(CurrentSongIndex));
				await PrepareNextTrack();
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		public async void PlaySongAtIndex(int index)
		{
			CurrentSongIndex = index;
			var song = GetSong(index);
			if (song == null)
			{
				return;
			}
			await NativePlayer.PlaySong(song);
			await PrepareNextTrack();
		}

		public async void MoveSong(int index, int newIndex)
		{
			try
			{
				var currSong = CurrentOrder[CurrentSongIndex];
				var songIndex = CurrentOrder[index];
				CurrentOrder.RemoveAt(index);
				CurrentOrder.Insert(newIndex, songIndex);
				SavePlaylist();
				if (index == CurrentSongIndex)
				{
					CurrentSongIndex = newIndex;
					await PrepareNextTrack();
				}
				CurrentSongIndex = Math.Max(0, CurrentOrder.IndexOf(currSong));

				NotificationManager.Shared.ProcCurrentPlaylistChanged();
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		public void RemoveSong(int index)
		{
			var songIndex = CurrentOrder[index];
			CurrentOrder.RemoveAt(index);
			Database.Main.Execute("delete from SongsOrdered where rowid = ?", songIndex + 1);
			CurrentPlaylistSongCount = CurrentOrder.Count;
			SavePlaylist();
			NotificationManager.Shared.ProcCurrentPlaylistChanged();
		}

		static BinaryFormatter BinaryFormatter = new BinaryFormatter();
		static CryptoRandom rnd = new CryptoRandom();

		public void ShuffleCurrentPlaylist()
		{
			if (Settings.CurrentPlaybackContext?.IsContinuous == false)
			{
				var start = DateTime.Now;
				for (int i = 0; i < CurrentOrder.Count; i++)
				{
					int pos = rnd.Next(i + 1);
					var x = CurrentOrder[i];
					CurrentOrder[i] = CurrentOrder[pos];
					CurrentOrder[pos] = x;
				}
				Console.WriteLine("Shuffle time {0}", (DateTime.Now - start).TotalSeconds);
			}
			SavePlaylist();
		}

		string plistLocation = Path.Combine(Locations.LibDir, "plist");

		void SavePlaylist()
		{
			if (CurrentOrder == null)
				return;
			if (!string.IsNullOrEmpty(plistLocation))
			{
				try
				{
					using (var s = File.OpenWrite(plistLocation))
					{
						BinaryFormatter.Serialize(s, CurrentOrder);
					}
				}
				catch (Exception ex)
				{
					LogManager.Shared.Report(ex);
				}
			}
		}


		async void LoadPlaylist()
		{
			if (string.IsNullOrEmpty(plistLocation) || !File.Exists(plistLocation)) return;
			try
			{
				using (var s = File.OpenRead(plistLocation))
				{
					var cs = BinaryFormatter.Deserialize(s) as List<int>;
					CurrentOrder = cs ?? new List<int>();
				}
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				//					SetupCurrentOrder(AudioPlayer.Shared.CurrentSongId());
			}
			CurrentPlaylistSongCount = Database.Main.ExecuteScalar<int>("select count(*) from SongsOrdered");
			var song = Database.Main.GetObject<Song, TempSong>(Settings.CurrentSong);
			if (song == null)
				return;
			CurrentSongIndex = Database.Main.ExecuteScalar<int>("select rowid -1 from SongsOrdered where Id = ?", song.Id);
			CurrentSongIndex = Math.Max(0, CurrentOrder.IndexOf(CurrentSongIndex));

			NativePlayer.CurrentSong = song;
			await Task.Run(()=>NativePlayer.PrepareFirstTrack(song, Settings.CurrentPlaybackIsVideo));

			if(Settings.CurrentPlaybackPercent > 0){
				Task.Run (async () => {
					while(true)
					{
						if(NativePlayer.Duration > 0){
							NativePlayer.Seek(Settings.CurrentPlaybackPercent);
							break;
						}
						else
							await Task.Delay(100);
					}
				});
			}
			await PrepareNextTrack();


			NotificationManager.Shared.ProcCurrentPlaylistChanged();
			//			if(CurrentOrder.Count < currentPlaylistSongCount)				
			//				SetupCurrentOrder(AudioPlayer.Shared.CurrentSongId());
		}

		public async Task PlayNext(MediaItemBase item)
		{
			var onlineSong = item as OnlineSong;
			if (onlineSong != null)
			{
				await MusicManager.Shared.AddTemp(onlineSong);
			}
			var song = item as Song;
			if (song != null)
			{
				PlayNext(song);
				return;
			}
			var artist = item as Artist;
			if (artist != null)
			{
				var songs = await MusicManager.Shared.GetSongs(artist);
				await PlayNext(songs);
				return;
			}

			var album = item as Album;
			if (album != null)
			{
				var songs = await MusicManager.Shared.GetSongs(album);
				await PlayNext(songs);
				return;
			}


			var genre = item as Genre;
			if (genre != null)
			{
				var songs = await MusicManager.Shared.GetSongs(genre);
				await PlayNext(songs);
				return;
			}

			App.ShowNotImplmented(new Dictionary<string, string> { { "Media Type", item?.GetType().ToString() } } );
		}

		public async void PlayNext(Song song)
		{
			var currIndex = Database.Main.ExecuteScalar<int>("select rowid from SongsOrdered where Id = ?", song.Id);
			if (currIndex > 0 && CurrentSongIndex > 0)
			{
				var songIndex = CurrentOrder.IndexOf (currIndex - 1);
				var offset = songIndex < CurrentSongIndex ? 0 : 1;
				MoveSong(songIndex, CurrentSongIndex + offset);
				return;
			}
			Database.Main.Insert(new SongsOrdered() { Id = song.Id });
			currIndex =
				Database.Main.ExecuteScalar<int>("select rowid -1 from SongsOrdered where Id = ? order by rowid desc LIMIT 1",
					song.Id);
			if (CurrentOrder.Count <= CurrentSongIndex + 1)
				CurrentOrder.Add(currIndex);
			else
				CurrentOrder.Insert(CurrentSongIndex + 1, currIndex);
			CurrentPlaylistSongCount = Database.Main.ExecuteScalar<int>("select count(*) from SongsOrdered");
			SavePlaylist();
			NotificationManager.Shared.ProcCurrentPlaylistChanged();
			await PrepareNextTrack();
		}

		public async Task PlayNext(List<Song> songs)
		{
			Database.Main.InsertAll(songs.Select(x => new SongsOrdered() { Id = x.Id }));
			var query =
				$"select rowid -1 as Value from SongsOrdered where Id in ('{string.Join("','", songs.Select(x => x.Id))}') order by rowid LIMIT {songs.Count}";
			var ids = Database.Main.Query<ReturnValue<int>>(query).Select(x => x.Value).ToArray();
			CurrentOrder.InsertRange(CurrentSongIndex + 1, ids);
			CurrentPlaylistSongCount = Database.Main.ExecuteScalar<int>("select count(*) from SongsOrdered");
			NotificationManager.Shared.ProcCurrentPlaylistChanged();
			PrepareNextTrack();
		}

		public async Task AddtoQueue(MediaItemBase item)
		{
			var onlineSong = item as OnlineSong;
			if (onlineSong != null)
			{
				await MusicManager.Shared.AddTemp(onlineSong);
			}
			var song = item as Song;
			if (song != null)
			{
				AddToQueue(song);
				return;
			}

			var artist = item as Artist;
			if (artist != null)
			{
				var songs = await MusicManager.Shared.GetSongs(artist);
				await AddToQueue(songs);
				return;
			}

			var album = item as Album;
			if (album != null)
			{
				var songs = await MusicManager.Shared.GetSongs(album);
				await AddToQueue(songs);
				return;
			}


			var genre = item as Genre;
			if (genre != null)
			{
				var songs = await MusicManager.Shared.GetSongs(genre);
				await AddToQueue(songs);
				return;
			}


			App.ShowNotImplmented(new Dictionary<string, string> { { "Media Type", item?.GetType().ToString() } });
		}

		public void AddToQueue(Song song)
		{
			Database.Main.Insert(new SongsOrdered() {Id = song.Id});
			var currIndex =
				Database.Main.ExecuteScalar<int>("select rowid -1 from SongsOrdered where Id = ? order by rowid desc LIMIT 1",
					song.Id);
			CurrentOrder.Add(currIndex);
			CurrentPlaylistSongCount = Database.Main.ExecuteScalar<int>("select count(*) from SongsOrdered");
			NotificationManager.Shared.ProcCurrentPlaylistChanged();
		}

		public async Task AddToQueue(List<Song> songs)
		{
			Database.Main.InsertAll(songs.Select(x => new SongsOrdered() {Id = x.Id}));
			var query = string.Format("select rowid -1 as Value from SongsOrdered where Id in ('{0}') order by rowid LIMIT {1}",
				string.Join("','", songs.Select(x => x.Id)), songs.Count);
			var ids = Database.Main.Query<ReturnValue<int>>(query).Select(x => x.Value).ToArray();
			CurrentOrder.AddRange(ids);
			CurrentPlaylistSongCount = Database.Main.ExecuteScalar<int>("select count(*) from SongsOrdered");
			NotificationManager.Shared.ProcCurrentPlaylistChanged();
		}
	}
}