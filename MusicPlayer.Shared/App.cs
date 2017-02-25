using System;
using System.Runtime.CompilerServices;
using System.Threading;
using MusicPlayer.Models;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Api.GoogleMusic;
using MusicPlayer.Managers;
using Xamarin;
using MusicPlayer.Data;
using System.Collections.Generic;
using Localizations;
#if !FORMS
using SimpleTables;
using MusicPlayer.Cells;
#endif

#if __IOS__

using MusicPlayer.iOS;

#elif __ANDROID__
using MusicPlayer.Droid;
#endif

namespace MusicPlayer
{
	public partial class App
	{
		static Thread MainThread;
		static Task startTask;
		static bool completed;

		public static async Task Start()
		{
			if (startTask == null || startTask.IsCompleted)
				startTask = RealStart();
			await startTask;
		}

		static async Task RealStart()
		{
			if (completed)
				return;
			MainThread = Thread.CurrentThread;
			InMemoryConsole.Current.Activate();
			TempFileManager.Shared.Cleanup();
			RegisterCells();
			var userData = Settings.CurrentUserDetails;
			if (userData != null)
			{
				LogManager.Shared.Identify(userData.Email);
			}
			completed = true;
			await NativeStart();
			await OfflineManager.Shared.DownloadMissingStuff();

		}

		static void RegisterCells()
		{
#if !FORMS
			CellRegistrar.Register<Song, SongCell>();
			CellRegistrar.Register<TempSong, SongCell>();
			CellRegistrar.Register<Artist, ArtistCell>();
			CellRegistrar.Register<Album, AlbumCell>();
			CellRegistrar.Register<TempAlbum, AlbumCell>();
			CellRegistrar.Register<TempArtist, ArtistCell>();
			CellRegistrar.Register<Genre, GenreCell>();
			CellRegistrar.Register<TempGenre, GenreCell>();
			CellRegistrar.Register<Playlist, PlaylistCell>();
			CellRegistrar.Register<PlaylistSong, PlaylistSongCell>();
			CellRegistrar.Register<RadioStation, RadioStationCell>();


			CellRegistrar.Register<OnlineSong, SongCell>();
			CellRegistrar.Register<OnlineAlbum, AlbumCell>();
			CellRegistrar.Register<OnlineArtist, ArtistCell>();
			CellRegistrar.Register<OnlinePlaylist, PlaylistCell>();
			CellRegistrar.Register<OnlinePlaylistEntry, PlaylistSongCell>();
			CellRegistrar.Register<OnlineRadioStation, RadioStationCell>();
#endif

		}

		public static Action<string, string> AlertFunction;

		public static void ShowAlert(string title, string message)
		{
			RunOnMainThread(() => AlertFunction(title, message));
		}

		public static void ShowNotImplmented(Dictionary<string,string> extra = null,
		                                     [CallerMemberName] string function = "",
		                                     [CallerFilePath] string sourceFilePath = "",
		                                     [CallerLineNumber] int sourceLineNumber = 0)
		{
			LogManager.Shared.LogNotImplemented(extra,function, sourceFilePath, sourceLineNumber);
			App.ShowAlert(Strings.Sorry, $"Coming soon: {function}");
		}

		public static Action<Action> Invoker;

		public static void RunOnMainThread(Action action)
		{
			if (Thread.CurrentThread == MainThread)
				action();
			else
				Invoker(action);
		}

		public static Action OnPlaying;

		public static void Playing()
		{
			OnPlaying?.Invoke();
		}

		public static Action OnStopped;

		public static void StoppedPlaying()
		{
			OnStopped?.Invoke();
		}

		public static Action<string> OnShowSpinner;

		public static void ShowSpinner(string title)
		{
			OnShowSpinner?.InvokeOnMainThread(title);
		}

		public static Action OnDismissSpinner;

		public static void DismissSpinner()
		{
			OnDismissSpinner?.InvokeOnMainThread();
		}

		public static Func<string,Task<bool>> OnCheckForOffline;
		public static async Task<bool> CheckForOffline()
		{
			if (!Settings.ShowOfflineOnly)
				return true;
			var s = await OnCheckForOffline (Strings.StartingARadioStationWillDisableOfflineMode);
			if(s)
				Settings.ShowOfflineOnly = false;
			return s;
		}

	}
}