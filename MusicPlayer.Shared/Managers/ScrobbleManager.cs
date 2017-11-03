using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using Xamarin;
using Lastfm.Scrobbling;
using Lastfm;
using MusicPlayer.Api;
using SimpleAuth;
using SimpleAuth.Providers;
#if __IOS__
using Accounts;
using MusicPlayer.iOS;
using Accounts;
using Foundation;
using Twitter;
using Localizations;
#endif

namespace MusicPlayer.Managers
{
	public class ScrobbleManager : ManagerBase<ScrobbleManager>
	{
		public ScrobbleManager()
		{
			try
			{
				Init();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		Lastfm.Session session;
		Connection connection;
		Lastfm.Scrobbling.ScrobbleManager manager;
		TwitterApi twitter = new TwitterApi("Twitter", ApiConstants.TwitterClientId, ApiConstants.TwitterSecret, new ModernHttpClient.NativeMessageHandler());
		public async void Init()
		{
			if (Settings.LastFmEnabled)
				await LoginToLastFm();
			if (Settings.TwitterEnabled)
				await LoginToTwitter();
			if (session?.Authenticated == true)
			{
				connection = new Connection(session);
				manager = new Lastfm.Scrobbling.ScrobbleManager(connection);
			}
		}

		public void LogOut()
		{
			session = null;
			connection = null;
			manager = null;
			Api.Utility.SetSecured("last.fm", "", ApiConstants.LastFmApiKey);
		}

		public async Task<bool> LoginToLastFm()
		{
			try
			{
				var existingData = Api.Utility.GetSecured("last.fm", ApiConstants.LastFmApiKey);
				if (!string.IsNullOrWhiteSpace(existingData))
				{
					session = existingData.ToObject<Session>();
					if (session.Authenticated)
						return true;
					if (string.IsNullOrWhiteSpace(session.APIKey) || string.IsNullOrWhiteSpace(session.APISecret))
					{
						session.APIKey = ApiConstants.LastFmApiKey;
						session.APISecret = ApiConstants.LastFmSecret;
					}
				}
				else
					session = new Session(ApiConstants.LastFmApiKey, ApiConstants.LastFmSecret);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				session = new Session(ApiConstants.LastFmApiKey, ApiConstants.LastFmSecret);
			}
			try
			{
				return await ShowLogin();
			}
			catch (Exception ex)
			{
				if (ex is TaskCanceledException)
					throw;
				LogManager.Shared.Report(ex);
			}

			return false;
		}

		async Task<bool> ShowLogin(string extra = "")
		{
			try
			{
				var credentials = await PopupManager.Shared.GetCredentials("Login to Last.FM", extra, "http://www.last.fm");
				await session.AuthenticateAsync(credentials.Item1, Utilities.MD5(credentials.Item2));
				if (session.Authenticated)
				{
					Api.Utility.SetSecured("last.fm", session.ToJson(), ApiConstants.LastFmApiKey);
					Init();
					return true;
				}
				return false;
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			return await ShowLogin("There was an error signing in");
		}


		public enum PlaybackEndedReason
		{
			PlaybackEnded,
			Skipped,
			StartedOver,
			Reverse,
		}

		public async void SetNowPlaying(Song song, string trackId)
		{
			if (!Settings.LastFmEnabled)
				return;
			await Task.Run(() => { nowPlaying(song, trackId); });
		}

		object locker = new object();

		void nowPlaying(Song song, string trackId)
		{
			var track = Database.Main.GetObject<Track, TempTrack>(trackId);
			var noPlayingTrack = new NowplayingTrack(song.Artist, song.Name,
				TimeSpan.FromSeconds((int)(track?.Duration ?? 0)));
			try
			{
				lock (locker)
				{
					manager.ReportNowplaying(noPlayingTrack);
				}
			}
			catch (Exception ex)
			{
				try
				{
					session.Authenticate(session.UserName, session.Password);
					lock (locker)
					{
						manager.ReportNowplaying(noPlayingTrack);
					}
				}
				catch (Exception e)
				{
					LogManager.Shared.Report(e);
				}
			}
		}

		public async void PlaybackEnded(PlaybackEndedEvent data)
		{
			await Task.Run(() => ProccesPlaybackEnded(data));
		}

		async Task ProccesPlaybackEnded(PlaybackEndedEvent data)
		{
			try
			{
				if (data.Position < 3)
					return;
				var song = Database.Main.GetObject<Song>(data.SongId);
				if (!string.IsNullOrWhiteSpace(song?.Id))
				{
					song.LastPlayed = (long)(DateTime.Now - new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
					Database.Main.Update(song);
				}
				var track = Database.Main.GetObject<Track, TempTrack>(data.TrackId);

				if (track == null)
				{
					track = await MusicManager.Shared.GetTrack(data.SongId);
					if (track == null)
					{
						LogManager.Shared.Report(new Exception("Null track during Record Track. This should not happen!!!"));
						return;
					}
					data.TrackId = track.Id;
				}
				track.PlayCount++;
				Database.Main.Update(track);
				data.Duration = track.Duration;
				data.ServiceType = track.ServiceType;

				LogManager.Shared.LogPlay(song);
				Task<bool> lastFmTask = null;
				Task<bool> recordPlayTask;
				List<Task<bool>> tasks = new List<Task<bool>>();
				if (Settings.LastFmEnabled)
					tasks.Add(lastFmTask = SubmitScrobbleToLastFm(song, data.Position, data.Duration));
				if (Settings.TwitterEnabled)
				{
					SubmitScrobbleToTwitter(song);
				}
				tasks.Add(recordPlayTask = MusicManager.Shared.RecordPlayback(data));
				await Task.WhenAll(tasks);

				data.Scrobbled = lastFmTask?.Result ?? false;
				data.Submitted = recordPlayTask?.Result ?? false;
				if (!data.Submitted || (!data.Scrobbled) && Settings.LastFmEnabled)
				{
					data.Save();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		async Task<bool> SubmitScrobbleToLastFm(Song song, double position, double duration)
		{
			try
			{
				if (position < 30)
				{
					return true;
				}
				manager.Queue(new Entry(song.Artist, song.Name, song.Album, DateTime.UtcNow, PlaybackSource.User,
										TimeSpan.FromSeconds(duration), ScrobbleMode.Played));
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}
		}

		public async Task<bool> LoginToTwitter()
		{
			try
			{
#if __IOS__
				if (Device.HasIntegratedTwitter)
				{
					return await LoginTwitterOld();
				}
#endif
				var account = await twitter.Authenticate();
				var user = await twitter.Get<Dictionary<string, string>>("account/verify_credentials.json");
				Settings.TwitterDisplay = user["name"];
				Settings.TwitterAccount = user["id"];
				return true;
			}
			catch (Exception x)
			{
				Console.WriteLine(x);
			}
			return false;
		}

		async Task<bool> LoginTwitterOld()
		{
#if __IOS__
			var store = new ACAccountStore();
			var accountType = store.FindAccountType(ACAccountType.Twitter);

			var success = false;
			var result = await store.RequestAccessAsync(accountType);
			success = result.Item1;
			if (!success)
			{
				Settings.TwitterEnabled = false;
				return false;
			}

			var accounts = store.FindAccounts(accountType);
			if ((accounts?.Length ?? 0) == 0)
			{
				Settings.TwitterEnabled = false;
				return false;
			}

			if (accounts?.Length == 1)
			{
				Settings.TwitterEnabled = true;
				var a = accounts[0];
				Settings.TwitterAccount = a.Identifier;
				Settings.TwitterDisplay = a.UserFullName;
				return true;
			}

			var sheet = new MusicPlayer.iOS.Controls.ActionSheet("Twitter");
			var sheetTask = new TaskCompletionSource<bool>();
			foreach (var a in accounts)
			{
				sheet.Add(a.Identifier, () =>
				{

					Settings.TwitterEnabled = true;
					Settings.TwitterAccount = a.Identifier;
					Settings.TwitterDisplay = a.UserFullName;
					sheetTask.TrySetResult(true);
				});
			}
			sheet.Add(Strings.Nevermind, null, true);
			sheet.Show(AppDelegate.window.RootViewController, AppDelegate.window.RootViewController.View);
#endif
			return false;
		}
		async Task<bool> SubmitScrobbleToTwitter(Song song)
		{

			var message = $"#NowPlaying {song.ToString(114)} on @gMusicApp";
			try
			{
				if (!Device.HasIntegratedTwitter)
				{
					var resp = await twitter.Post(null, "statuses/update.json", new Dictionary<string, string>()
					{
						{"status",message}
					});
					Console.WriteLine(resp);
				}
				else
				{
#if __IOS__
					var store = new ACAccountStore();
					//var accountType = store.FindAccountType(ACAccountType.Twitter);
					var account = store.FindAccount(Settings.TwitterAccount);


					var request = new TWRequest(new NSUrl("https://api.twitter.com/1.1/statuses/update.json"), NSDictionary.FromObjectAndKey((NSString)message, (NSString)"status"), TWRequestMethod.Post);
					request.Account = account;
					var result = await request.PerformRequestAsync();

#endif
				}
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return false;
			}
		}
	}
}